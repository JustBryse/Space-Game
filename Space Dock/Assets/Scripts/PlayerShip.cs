using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour {

    public static int psLayer = 9;

    public AudioClip targetedSound;
    public Torpedo torpedoPrefab;
    public Bullet bulletPrefab;
    public Bullet railGunBulletPrefab;

    public List<Transform> pdcPositions = new List<Transform>(); // the pdc spawn points
    public Transform torpedoTube; // this is not the torpedo module but just the torpedo spawn point
    public Transform railGun; // the location where the railgun bullets will spawn
    public GameObject sensorSigCircle;

    public float startHitPoints = 300f;
    public float blindSpotAngle = 60f;
    public float maxRadarSig = 2000f;
    public float startRadarSig = 1000f; // the base range the playership can be to an AI ship before being detected. Firing thruster or weapons will shorten this range
    public float rsCoolDown = 3f; // the time that must past before the radarSig can go down
    public float rsRecovery = 1f; // the rate at which radarSig goes down
    // radar sig mods represent how much larger the radar sig will become when certain events happen
    public float tpdRS = 50f;
    public float pdcRS = 1f;
    public float thrustRS = 100f;

    public int torpMagSize = 3;
    public int pdcMagSize = 120;
    public float pdcReloadTime = 3f;
    public float torpReloadTime = 5f;
    public float pdcFireRate = 0.1f;
    public float pdcMuzzleSpd = 100f;

    //public int railGunMagSize = 5;
    //public float railGunFireRate = 0.1f;
    //public float railGunMuzzleSpd = 100;

    public int maxModuleRepairCount = 1;

    // stores the current amount of ammo in the mag
    int torpMag;
    int pdcMag;

    float hitPoints;
    float radarSig;
    float minRadarSig;
    float rsTimer; // the radarSig cool down timer
    float pdcTimer; // the fire rate counter for the pdc
    //float railGunTimer; // the fire rate counter for the railgun

    bool torpSafety = false; // flag controlling if torpedos can be fired
    bool pdcSafety = true; // the flag that controls if the pdcs can be fired or not. The pdcs start being off
    bool pdcTrigger = false; // the trigger on the pdc gun. The player pulls or releases this trigger by clicking down and up on the screen
    //bool railGunSafety = false; // the flag that controls if the railgun can be fired
    bool targeted = false; // a flag that tells if the player ship if he has been target locked

    // a crewman is associated to a job which is associated to a module
    List<Crewman> crew = new List<Crewman>();
    List<Job> jobs = new List<Job>();
    public List<Module> modules = new List<Module>(); // stores all the modules for this ship (main drive, torpedo tube, sensor array, etc...)
    Module sensorModule;

    Rigidbody rb;
    UIManager uim;
    LevelManager lm;
    PlayerController pc;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        uim = UIManager.FindObjectOfType<UIManager>();
        pc = GetComponent<PlayerController>();
        lm = LevelManager.FindObjectOfType<LevelManager>();

        foreach (Module mod in modules)
        {
            if (mod.moduleType == Module.ModuleType.Sensor)
            {
                sensorModule = mod;
            }
        }

        generateRandomCrew(5);

        torpMag = torpMagSize;
        pdcMag = pdcMagSize;
        pdcTimer = pdcFireRate;
        //railGunTimer = railGunFireRate;
        hitPoints = startHitPoints;
        radarSig = startRadarSig;
        minRadarSig = startRadarSig;
        rsTimer = rsCoolDown;

        uim.setPDCMagSlider(pdcMagSize, pdcMag);
        uim.setTorpedoMagSlider(torpMagSize, torpMag);
    }

    // Update is called once per frame
    void Update() {
        decreaseRadarSig();
        firePDC();
        //fireRailGun(); // the rail gun is a WIP. I may not keep it in the game
    }

    // called by the UIManager when the player clicks down on the firePDCPanel
    public void firePDC()
    {
        if (pdcSafety == false && pdcTimer <= 0f && pdcTrigger == true) // the trigger is pulled while the safety is off and the next round is chambered
        {
            foreach (Transform launchPos in pdcPositions)
            {
                // make sure the pdc module on this pdc Transform is online
                if (launchPos.GetComponent<Module>().isOnline() && pdcMag > 0)
                {
                    Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
                    targetPos.z = 0f;
                    Vector3 force = (targetPos - launchPos.position).normalized * pdcMuzzleSpd * Time.deltaTime / 25f;
                    GameObject bullet = (GameObject)Instantiate(bulletPrefab.gameObject, launchPos.position, Quaternion.identity);

                    bullet.GetComponent<Bullet>().setSpawner(gameObject);
                    Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
                    bulletRB.velocity = rb.velocity;
                    //bulletRB.AddForce(force);
                    bullet.GetComponent<Bullet>().setStartVelocity(rb.velocity + force);

                    Vector3 direction = (targetPos - bullet.transform.position).normalized;

                    float directionAngle = Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg; // direction angle in degrees

                    if (targetPos.x < bullet.transform.position.x)
                    {
                        directionAngle += 90f;
                    }
                    else
                    {
                        directionAngle -= 90f;
                    }

                    bullet.transform.eulerAngles = new Vector3(0f, 0f, directionAngle);
                    pdcMag--;
                    uim.setPDCMagSlider(pdcMagSize, pdcMag);

                    increaseRadarSig(pdcRS, false);
                }
            }

            pdcTimer = pdcFireRate;
        }
        else if (pdcTimer > 0)
        {
            pdcTimer -= Time.deltaTime;
        }
    }

    /*
    public void fireRailGun()
    {
        if (railGunSafety == false && railGunTimer <= 0f && Input.GetKeyDown(KeyCode.F) && railGun.GetComponent<Module>().isOnline())
        {
            GameObject bullet = (GameObject)Instantiate(railGunBulletPrefab.gameObject, railGun.position, transform.rotation);
            Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();

            Vector3 force = transform.up * railGunMuzzleSpd * Time.deltaTime;
            bulletRB.velocity = rb.velocity;
            bulletRB.AddForce(force);

            railGunTimer = railGunFireRate;
        }
        else
        {
            railGunTimer -= Time.deltaTime;
        }
    }
    */

    public void setPDCSafety(bool flag)
    {
        pdcSafety = flag;
    }

    public bool getPDCSafety()
    {
        return pdcSafety;
    }

    public void fireTorpedo(Tangible target)
    {
        // both torpedo tube and sensors must work to fire torpedos
        if (torpedoTube.GetComponent<Module>().isOnline() && sensorModule.isOnline() && torpSafety == false && torpMag > 0)
        {
            GameObject torp = (GameObject)Instantiate(torpedoPrefab.gameObject, torpedoTube.position, Quaternion.identity);
            torp.GetComponent<Rigidbody>().velocity = rb.velocity;
            torp.GetComponent<Torpedo>().setTarget(target.transform);

            torpMag--;
            uim.setTorpedoMagSlider(torpMagSize, torpMag);
            increaseRadarSig(tpdRS, true);
        }
    }

    private void generateRandomCrew(int crewCount)
    {
        for (int i = 0; i < crewCount - 1; i++)
        {
            int value = Random.Range(0, 3);
            Crewman.Professions profession = Crewman.Professions.Any;

            if (value == 0)
            {
                profession = Crewman.Professions.Pilot;
            }
            else if (value == 1)
            {
                profession = Crewman.Professions.Marine;
            }
            else if (value == 2)
            {
                profession = Crewman.Professions.Engineer;
            }

            ResourcesManager resMan = ResourcesManager.FindObjectOfType<ResourcesManager>();
            value = Random.Range(0, resMan.crewmanPortraits.Count);
            Sprite portrait = resMan.crewmanPortraits[value];
            string name = "Crewman " + Random.Range(0,1000);

            Crewman crewman = new Crewman(portrait, name, profession, Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3));
            crew.Add(crewman);
        }
    }

    public List<Crewman> getCrew()
    {
        return crew;
    }

    public List<Module> getModules()
    {
        return modules;
    }

    public int getMaxModuleRepairCount()
    {
        return maxModuleRepairCount;
    }

    // reloads the entire pdc mag. Note that the UIM calls this function and checks if a reload is possible first
    public IEnumerator reloadPDCMag()
    {
        yield return new WaitForSeconds(pdcReloadTime);
        pdcMag = pdcMagSize;
        uim.setPDCMagSlider(pdcMag, pdcMagSize);
        uim.finishPDCReload();
    }

    // reloads a single torpedo. Note that the UIM calls this function and checks if a reload is possible first. The
    public IEnumerator reloadTorpedoMag()
    {
        torpSafety = true; // turn off torpedo launcher
        yield return new WaitForSeconds(torpReloadTime);
        torpMag++;
        uim.setTorpedoMagSlider(torpMagSize, torpMag);
        uim.finishTorpedoReload();
        torpSafety = false; // turn on torpedo launcher
    }

    public bool canReloadPDCMag()
    {
        if (pdcMag < pdcMagSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool canReloadTorpedoMag()
    {
        if (torpMag < torpMagSize && torpedoTube.GetComponent<Module>().isOnline())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // has a chance to damage a ship module
    // critMod is a value between zero and one and potentially increases the odds of disabling a module
    public void receiveDamage(float damage, float critMod)
    {
        hitPoints -= damage;

        // the player is dead if this condition is reached
        if (hitPoints <= 0)
        {
            hitPoints = 0;
            // go back to start scene upon being destroyed
            lm.loadStart();
        }

        // choose a random module and roll the dice to see if it is damaged
        int index = Random.Range(0, modules.Count - 1);
        Module module = modules[index];
        if (module.isOnline()) // only attack module if it is currenly functioning
        {
            module.recieveDamage(critMod);
        }

        uim.setPlayerHitPointsSlider(hitPoints, startHitPoints);
        uim.setShipInfoText(rb.velocity, transform);
    }

    // while not emitting emissions, the radar sig will go down
    // called form update()
    void decreaseRadarSig()
    {
        if (radarSig > minRadarSig && rsTimer <= 0)
        {
            radarSig -= rsRecovery * Time.deltaTime;
        }
        else
        {
            rsTimer -= Time.deltaTime;
        }

        // update the scale of the sensor sig radius object
        sensorSigCircle.transform.localScale = new Vector3(radarSig*2,radarSig*2,0);
    }

    // called whenever a thruster or weapon is fired or detectable emissions are released
    public void increaseRadarSig(float ping, bool instantly)
    {
        // reset the timer so that the player will have to wait for the radarSig to lessen
        rsTimer = rsCoolDown;

        // don't go past a certain radarSig
        if (radarSig < maxRadarSig)
        {
            if (instantly) // instantly increment radarSig without Time.deltaTime
            {
                radarSig += ping;
            }
            else
            {
                radarSig += ping * Time.deltaTime;
            }
        }
        else
        {
            radarSig = maxRadarSig;
        }

        // update the scale of the sensor sig radius object
        sensorSigCircle.transform.localScale = new Vector3(radarSig*2,radarSig*2,0);
    }

    public float getThrusterRS()
    {
        return thrustRS;
    }

    public float getRadarSig()
    {
        return radarSig;
    }

    public bool isTargetLocked()
    {
        return targeted;
    }

    public void setTargeted(bool flag)
    {
        // if we were stealth but now targeted, sound the alarm
        if (targeted == false && flag == true)
        {
            float volume = 1f / (float)Camera.main.orthographicSize;
            //AudioSource.PlayClipAtPoint(targetedSound, Camera.main.transform.position, volume);
        }

        targeted = flag;
        uim.setShipInfoText(rb.velocity,transform);
    }

    public float getHitpoints()
    {
        return hitPoints;
    }

    public float getMaxHitPoints()
    {
        return startHitPoints;
    }

    // see if the player ship can actually see the target. This is so that AI ships can hide in the radar shadow of other ships and in the radar blind spot of the player ship
    public bool canDetectTarget(Transform target)
    {
        // friendly torpedos will not be in the same raycast layer as AI colliders so that they dont block raycasting of other targets
        // I imagine it as friendly torpedos relaying signals forward and back to the player ship or something like that
        if (target.gameObject.layer == Torpedo.playerTPDLayer)
        {
            Vector3 tpdDir = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(tpdDir, -transform.up);
            bool hiddenInBlindSpot = angle <= blindSpotAngle / 2f && pc.thrusterExhaust.isPlaying;
            return !hiddenInBlindSpot;
        }

        RaycastHit rh;
        Vector3 dir = (target.position - transform.position).normalized;
        int layer = LayerMask.GetMask("AICollider");
        if(Physics.Raycast(transform.position,dir, out rh, Mathf.Infinity, layer))
        {
            float angle = Vector3.Angle(dir, -transform.up);

            bool hiddenInBlindSpot = angle <= blindSpotAngle/2f && pc.thrusterExhaust.isPlaying; // if the target is in the blind spot while the main drive is firing, then the target will be hidden
            bool hitTarget = rh.collider.transform == target;

            return !hiddenInBlindSpot && hitTarget; // if the target is not hidden in the blind spot and the target is in line of sight, then it is detectable
        }

        return false; // return false if something goes wrong
    }

    public void setPDCTrigger(bool flag)
    {
        pdcTrigger = flag;
    }

    public bool getPDCTrigger()
    {
        return pdcTrigger;
    }

    public void setMinRadarSig(float sig)
    {
        minRadarSig = sig;
    }

    public float getStartRadarSig()
    {
        return startRadarSig;
    }

    public void setRadarSig(float sig)
    {
        radarSig = sig;
    }

    public float getMaxRadarSig()
    {
        return maxRadarSig;
    }

    public Module getSensorModule()
    {
        return sensorModule;
    }

    // toggles the visible range of the player's sensor emissions
    public void toggleSensorSigCircle()
    {
      bool flag = sensorSigCircle.activeInHierarchy;
      sensorSigCircle.SetActive(!flag);
    }
}

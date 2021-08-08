using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class AIController : MonoBehaviour {

    //public GameObject targetMarker;
    public GameObject thrusterExhaust;
    public GameObject ccwRCSExhaust;
    public GameObject cwRCSExhaust;
    public Bullet pdcBullet;
    public Torpedo torpedoPrefab;
    public ParticleSystem deathExplosion;

    public enum Attitudes {stalking, guarding}
    public Attitudes attitude;

    public Transform torpedoTube;
    public List<Transform> pdcPositions = new List<Transform>();

    public float startHitPoints = 100f;
    public float blindSpotAngle = 90; // radar blind spot angle that is behind the ship in the direction of its thruster exhaust
    public float stalkerGiveUpDelay = 5f; // the time that must pass before a wandering stalker AIController gives up looking for the player and changes to a more broad wandering pattern
    public float stalkerWanderRange = 10f;
    public float guardWanderRange = 100f;
    public float disengageDelay = 5f;
    public int tpdMagSize = 3;
    public float tpdFireRate = 0.125f; // torpedos can be fired in rapid succession. The entire tpd mag can be emptied quickly
    public float tpdReloadSpd = 3f;
    public float tpdMaxRange = 900f;
    public float tpdMinRange = 300f;

    public int pdcMagSize = 3;
    public float pdcFireRate = 0.1f;
    public float pdcMuzzleSpd = 1000f;
    public float maxPDCSpread = 0f; // controls how accurate the pdcs are
    public float pdcReloadSpd = 3f;
    public float pdcRange = 50f;

    public float minStalkRange = 75f;
    public float maxStalkRange = 100f;

    public float thrust = 2000f;
    public float orbitThrust = 750f;
    public float normalAngularThrust = 50f;
    public float wanderAngularThrust = 25f;

    Rigidbody rb;
    PlayerShip ps;
    Vector3 startPosition;
    Vector3 lastPlayerPosition;
    Vector3 wanderDestination;

    float hitPoints = 0f;
    float disengageTimer;

    float tpdTimer = 0f;
    bool tpdSafety = false;
    float tpdMag = 0f;

    float pdcTimer = 0f;
    bool pdcSafety = true;
    bool pdcReloading = false;
    bool engaged = false; // true when firing weapons
    float pdcMag = 0f;
    float angularThrust;
    float psLastContactTime = 0; // the last point in time when the player was detected

    Queue<Transform> targetQueue = new Queue<Transform>(); // add in targeting queue later
    Transform currentTarget; // stores the current target that the pdcs will shoot at when in range

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
        hitPoints = startHitPoints;
        angularThrust = normalAngularThrust;
        disengageTimer = disengageDelay;
        tpdMag = tpdMagSize;
        pdcMag = pdcMagSize;
        lastPlayerPosition = transform.position; // I am doing this so that the AI will wander in place where it spawned if the player isn't detected right away just so I don't get null pointer exceptions
        startPosition = transform.position;
        psLastContactTime = Time.time - stalkerGiveUpDelay; // we don't want the AI controller to be waiting to give up from the beginning of the game. It needs to detect the player first
    }

	// Update is called once per frame
	void Update () {
        // only engage in combat behaviour if the player is detected
        assignCurrentTarget();

        if (canDetectTarget(ps.transform))
        {
            // the AI will always engage and pursue the player
            if (attitude == Attitudes.stalking || attitude == Attitudes.guarding)
            {
                shadowPlayer(minStalkRange, maxStalkRange);
                fireTorpedo();
            }
        }
        else
        {
            //enage in idle behaviour
            wander();
        }

        if (canDetectTarget(currentTarget))
        {
            firePDCs();
        }

        setDisengageTimer();
    }

    bool canDetectTarget(Transform target)
    {
        if (target)
        {
            // calculate if the target is in the blind spot angle

            Vector3 targetDir = (target.position - transform.position).normalized;
            Vector3 rearDir = -transform.up;

            float targetAngle = Vector3.Angle(rearDir, targetDir);
            bool hiddenInBlindSpot = targetAngle <= blindSpotAngle / 2f; // check if the target is hidden in the rear of this AI ship

            // if the current target is the player than we have to make sure that the players radarSig is in range also
            if (target.GetComponent<PlayerShip>())
            {
                float distance = Vector3.Distance(target.position, transform.position);
                bool inRange = distance < ps.getRadarSig(); // check if the player is in detection range

                RaycastHit rh;
                // check if the player is in line of sight (other AI ships won't block sensors) because the player is on another layer
                if (Physics.Raycast(transform.position, targetDir, out rh, Mathf.Infinity, LayerMask.GetMask("PlayerStuff")))
                {
                    bool result = rh.collider.GetComponent<PlayerShip>() && !hiddenInBlindSpot && inRange; // if in range, and not in blind spot, and is the player

                    if (result)
                    {
                        psLastContactTime = Time.time; // recording the point in time when the player was detected
                        setLastPlayerPos();
                    }

                    ps.setTargeted(result);
                    return result;
                }
                else
                {
                    return false; // return false if something goes wrong
                }

            }
            else if (target.GetComponent<Torpedo>())
            {
                RaycastHit rh;
                if (Physics.Raycast(transform.position, targetDir, out rh, Mathf.Infinity, LayerMask.GetMask("PlayerStuff")))
                {
                    if (rh.collider.GetComponent<Torpedo>())
                    {
                        return !hiddenInBlindSpot && rh.collider.GetComponent<Torpedo>().getHarmsPlayer() == false; // if it is a playerTorpedo and not in the blind spot
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false; // return false if the ray cast didn't work
                }
            }
            else // if the target is something else than we only care if its outside the radar blind spot
            {
                return !hiddenInBlindSpot;
            }
        }
        else
        {
            return false; // return false if the target is null
        }
    }

    // called from update and used to keep track of where the player is while he is detectable.
    void setLastPlayerPos()
    {
        lastPlayerPosition = ps.transform.position;

        if (attitude == Attitudes.stalking)
        {
            wanderDestination = lastPlayerPosition;
        }
    }

    // a timer will countdown while disengaged. When it reaches zero the AI can do a disengaged reload
    void setDisengageTimer()
    {
        //print("disengage timer: " + disengageTimer);
        //print("pdc mag: " + pdcMag);
        //print("tpd mag: " + tpdMag);
        if (engaged)
        {
            disengageTimer = disengageDelay;
        }
        else
        {
            disengageTimer -= Time.deltaTime;
        }
    }

    void wander()
    {
        float wanderDistance = Vector3.Distance(wanderDestination, transform.position);

        // for reasons I am unaware of, the wander distance can sometimes be really huge. It should never be higher than the max radar sig of the player
        // if the wander distance is reset than the folowing section of this function will choose a new wander destination that makes sense
        if (wanderDistance > ps.getMaxRadarSig())
        {
            wanderDistance = 0;
        }

        angularThrust = wanderAngularThrust;
        // set a new wander destination if close to it
        if (wanderDistance <= 5f)
        {
            // wander towards the last known position of the player if stalking
            if (attitude == Attitudes.stalking)
            {
                // if the player was detected recently than pursure his last know position
                if (Time.time - psLastContactTime < stalkerGiveUpDelay)
                {
                    // move towards the player's last known position with less varience to pursure the player with a more narrow scope of search
                    wanderDestination = lastPlayerPosition;
                    //print("currently narrow wandering");
                }
                else // if the player hasn't been seen in a while than wander around in the general area of the players last known position
                {
                    // move towards the players last known position with broad variance to expand the scope of the search
                    float x = Random.Range(-stalkerWanderRange, stalkerWanderRange);
                    float y = Random.Range(-stalkerWanderRange, stalkerWanderRange);
                    wanderDestination = new Vector3(x, y, 0) + lastPlayerPosition;
                    //print("currently broad wandering");
                }
            }
            else if (attitude == Attitudes.guarding) // wander around the place where the AI was first spawned as if guarding
            {
                wanderDestination = new Vector3(Random.Range(-guardWanderRange, guardWanderRange), Random.Range(-guardWanderRange, guardWanderRange), 0) + startPosition;
            }
        }
        else
        {
            Vector3 force = (wanderDestination - transform.position).normalized * (thrust/4f) * Time.deltaTime;
            rb.AddForce(force);
            toggleDriveThrusters(true);
            alignToThrust(force);

            // don't forget to reload the tpd mag while wondering because the fireTorpedo() function isn't being called while wandering
            if (tpdMag < tpdMagSize && tpdSafety == false && disengageTimer <= 0f)
            {
                StartCoroutine(reloadTorpedoMag());
            }

            // try to reload the pdcs if disengaged
            if (pdcMag < pdcMagSize && pdcReloading == false && disengageTimer <= 0f)
            {
                StartCoroutine(reloadPDCMag());
            }
        }
    }

    // activates pdcs if there is a current target
    void assignCurrentTarget()
    {
        if (pdcReloading == false) // only assign targets if the pdc is not reloading
        {
            // sometimes the current target will move out of range but the AI is still waiting for a firing solution that will never happen unless the torpedo returns into range
            if (currentTarget != null)
            {
                float targetDist = Vector3.Distance(transform.position, currentTarget.position);

                if (targetDist > pdcRange + 5f)
                {
                    // clear current target because it has moved out of range
                    currentTarget = null;
                }
            }

            // deciding whether to select a new target and turn off pdc safety
            if (currentTarget != null)
            {
                pdcSafety = false;
            }
            else
            {
                // if current target is null and the target is empty than turn off pdcs
                if (targetQueue.Count == 0)
                {
                    pdcSafety = true;
                }
                // the target queue still has targets
                else
                {
                    currentTarget = targetQueue.Dequeue();
                    pdcSafety = false;
                }
            }

            /* if the current target is the player ship but there are other items in the target queue (such as torpedos), then move the player to the end of the queue and target
               the next item in the queue */
            if (currentTarget)
            {
                if (currentTarget.GetComponent<PlayerShip>() && targetQueue.Count > 0)
                {
                    targetQueue.Enqueue(currentTarget);
                    currentTarget = targetQueue.Dequeue();
                }

                // we must check to see if the current target is even detectable. A missile in the ships blind spot angle should not be targeted for example.
                // I would shape colliders in nice pie cut-out shapes if I could but this is the best I can do.
                // Targets in the blind spot will be put back into the queue until they are no longer in the blind spot
                if (canDetectTarget(currentTarget) == false && targetQueue.Count > 0)
                {
                    targetQueue.Enqueue(currentTarget);
                    currentTarget = targetQueue.Dequeue();
                }
            }
        }
    }

    void fireTorpedo()
    {
        float distance = Vector3.Distance(ps.transform.position, transform.position);

        if (tpdSafety == false && tpdTimer <= 0 && tpdMag > 0 && distance <= tpdMaxRange && distance >= tpdMinRange) // we dont have to check for range because the AITPDSensor sets the tpd sensor when its collider detects the playership
        {
            GameObject torpedo = (GameObject)Instantiate(torpedoPrefab.gameObject, torpedoTube.position, transform.rotation);
            torpedo.GetComponent<Torpedo>().setTarget(ps.transform);

            tpdTimer = tpdFireRate;
            tpdMag--;
            engaged = true;

            if (tpdMag <= 0)
            {
                StartCoroutine(reloadTorpedoMag());
            }
        }
        /*
        else if (tpdSafety == false && tpdMag < tpdMagSize && (distance < tpdMinRange || distance > tpdMaxRange)) // reload torpedo mag if player is not within tpd range
        {
            StartCoroutine(reloadTorpedoMag());
        }
        */
        else
        {
            engaged = false;
        }

        tpdTimer -= Time.deltaTime;
    }

    IEnumerator reloadTorpedoMag()
    {
        tpdSafety = true;

        yield return new WaitForSeconds(tpdReloadSpd);

        if (tpdMag < tpdMagSize)
        {
            tpdMag++;
        }

        tpdSafety = false;
    }

    void firePDCs()
    {
        if (pdcSafety == false)
        {
            //print(currentTarget); // what is the current target
            Vector3 target = currentTarget.position;
            float distance = (target - transform.position).magnitude;

            if (pdcTimer <= 0 && distance <= pdcRange)
            {
                foreach (Transform pos in pdcPositions)
                {
                    if (pdcMag > 0)
                    {
                        Vector3 tv = currentTarget.GetComponent<Rigidbody>().velocity; // target velocity
                        float bs = pdcMuzzleSpd * Time.deltaTime; // bullet speed
                        Vector3 d = pos.position - currentTarget.position; // displacement between target and bullet positions
                        float cosTheta = Vector3.Dot(d.normalized, tv.normalized); // the cosine of the angle between the displacement and target velocity vectors

                        float a = Mathf.Pow(bs, 2) - Mathf.Pow(tv.magnitude, 2);
                        float b = 2 * cosTheta;
                        float c = -Mathf.Pow(d.magnitude, 2);

                        // solve for intercept solution time with quadratic formula

                        float t = 0; // the chosen firing solution time

                        if (Mathf.Pow(b, 2) - (4 * a * c) >= 0) // if this is greater than zero than there exists a firing solution. It may exist in the past though
                        {
                            float t1 = (-b + Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c))) / (2 * a);
                            float t2 = (-b - Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c))) / (2 * a);

                            if (t1 < 0 && t2 < 0) // if both intercept times are in the past than there is no viable firing solution
                            {
                                return;
                            }
                            else if (t1 < 0 && t2 >= 0)
                            {
                                t = t2;
                            }
                            else if (t1 >= 0 && t2 < 0)
                            {
                                t = t1;
                            }
                            else if (t1 >= 0 && t2 >= 0)
                            {
                                t = Mathf.Min(t1, t2);
                            }
                        }
                        else
                        {
                            // no solution
                            return;
                        }

                        // extrapolating less is needed when shooting at torpedos (I don't know why)
                        if(currentTarget.GetComponent<Torpedo>())
                        {
                          t = 0.125f*t;
                        }

                        Vector3 ip = (tv * t) + currentTarget.position; // extrapolate the intercept position (future position) of the target at time "t"

                        // spawn something at the intercept point
                        //GameObject marker = Instantiate(targetMarker,ip,Quaternion.identity) as GameObject;
                        //Destroy(marker,3f);

                        Vector3 bulletForce = (ip - pos.position).normalized * bs;
                        Vector3 bulletSpread = new Vector3(Random.value * maxPDCSpread, Random.value * maxPDCSpread, 0f);
                        bulletForce += bulletSpread;

                        GameObject bullet = (GameObject)Instantiate(pdcBullet.gameObject, pos.position, Quaternion.identity);
                        bullet.transform.up = bulletForce.normalized; // align the bullet to its launch vector
                        bullet.GetComponent<Bullet>().setSpawner(gameObject);

                        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
                        if (rb.velocity.magnitude > 10f) // only add the ship velocity vector if moving fast
                        {
                            bulletRB.velocity = rb.velocity + bulletForce;
                            bullet.GetComponent<Bullet>().setStartVelocity(rb.velocity + bulletForce);
                        }
                        else
                        {
                            bulletRB.velocity = bulletForce;
                            bullet.GetComponent<Bullet>().setStartVelocity(bulletForce);
                        }
                        //bulletRB.AddForce(bulletForce);

                        pdcMag--;
                        engaged = true;
                    }
                    else
                    {
                        // reload and exit firing loop
                        StartCoroutine(reloadPDCMag());
                        break;
                    }
                }

                pdcTimer = pdcFireRate;
            }
            else
            {
                engaged = false;
            }

            /*
            // if the player is out of range then reload if needed
            if (pdcMag < pdcMagSize && distance > pdcRange && pdcReloading == false && disengageTimer <= 0f)
            {
                StartCoroutine(reloadPDCMag());
            }
            */
        }

        pdcTimer -= Time.deltaTime;
    }

    IEnumerator reloadPDCMag()
    {
        pdcReloading = true;
        pdcSafety = true;
        yield return new WaitForSeconds(pdcReloadSpd);
        pdcMag = pdcMagSize;
        pdcSafety = false;
        pdcReloading = false;
    }

    void toggleDriveThrusters(bool flag)
    {
        foreach (Transform thruster in thrusterExhaust.transform)
        {
            ParticleSystem thrusterPS = thruster.GetComponent<ParticleSystem>();
            //AudioSource thrusterAS = thruster.GetComponent<AudioSource>();

            if (flag)
            {
                if (thrusterPS.isPlaying == false)
                {
                    thrusterPS.Play();
                }

                /*
                if (thrusterAS.isPlaying == false)
                {
                    thrusterAS.Play();
                }
                */
            }
            else
            {
                thrusterPS.Stop();
                //thrusterAS.Stop();
            }
        }
    }

    void toggleCCWRCS(bool flag)
    {
        foreach (Transform rcs in ccwRCSExhaust.transform)
        {
            ParticleSystem rcsPS = rcs.GetComponent<ParticleSystem>();
            //AudioSource rcsAS = rcs.GetComponent<AudioSource>();

            if (flag)
            {
                if (rcsPS.isPlaying == false)
                {
                    rcsPS.Play();
                }
                /*
                if (rcsAS.isPlaying == false)
                {
                    rcsAS.Play();
                }
                */
            }
            else
            {
                rcsPS.Stop();
                //rcsAS.Stop();
            }
        }
    }

    void toggleCWRCS(bool flag)
    {
        foreach (Transform rcs in cwRCSExhaust.transform)
        {
            ParticleSystem rcsPS = rcs.GetComponent<ParticleSystem>();
            //AudioSource rcsAS = rcs.GetComponent<AudioSource>();

            if (flag)
            {
                if (rcsPS.isPlaying == false)
                {
                    rcsPS.Play();
                }

                /*
                if (rcsAS.isPlaying == false)
                {
                    rcsAS.Play();
                }
                */
            }
            else
            {
                rcsPS.Stop();
                //rcsAS.Stop();
            }
        }
    }

    // try to stay a certain distance from the player
    void shadowPlayer(float minRange, float maxRange)
    {
        angularThrust = normalAngularThrust;
        Vector3 direction = ps.transform.position - transform.position;
        Vector3 force = direction.normalized * thrust * Time.deltaTime;

        Vector3 aimPoint = ps.transform.position;

        if (direction.magnitude < minRange) // move away from player
        {
            force = -force;
            rb.AddForce(force);

            toggleDriveThrusters(true);
        }
        else if (direction.magnitude > maxRange) // move closer to player
        {
            rb.AddForce(force);

            toggleDriveThrusters(true);
        }
        else if (direction.magnitude > minRange && direction.magnitude < maxRange) // circle the player
        {
            // returns a vector in the x-y plane that is perpendicular to the direction vector
            Vector3 normal = Vector3.Cross(direction, Vector3.forward).normalized;
            force = normal * orbitThrust * Time.deltaTime;
            rb.AddForce(force);

            toggleDriveThrusters(true);
        }
        else
        {
            toggleDriveThrusters(false);
        }

        alignToThrust(force);
    }

    void alignToThrust(Vector3 force)
    {
        float forceAngle = Vector3.Angle(force, Vector3.right);
        if (force.y < 0)
        {
            forceAngle = 360f - forceAngle;
        }

        float shipAngle = Vector3.Angle(transform.up, Vector3.right);
        if (transform.up.y < 0)
        {
            shipAngle = 360f - shipAngle;
        }

        if (Mathf.Abs(forceAngle - shipAngle) > 10)
        {
            if (forceAngle > shipAngle)
            {
                if (forceAngle - shipAngle < shipAngle + (360f - forceAngle))
                {
                    // rotate ccw
                    transform.eulerAngles += Vector3.forward * angularThrust * Time.deltaTime;
                    toggleCCWRCS(true);
                    toggleCWRCS(false);
                }
                else
                {
                    // rotate cw
                    transform.eulerAngles -= Vector3.forward * angularThrust * Time.deltaTime;
                    toggleCCWRCS(false);
                    toggleCWRCS(true);
                }
            }
            else
            {
                if (shipAngle - forceAngle < forceAngle + (360f - shipAngle))
                {
                    // rotate cw
                    transform.eulerAngles -= Vector3.forward * angularThrust * Time.deltaTime;
                    toggleCCWRCS(false);
                    toggleCWRCS(true);
                }
                else
                {
                    // rotate ccw
                    transform.eulerAngles += Vector3.forward * angularThrust * Time.deltaTime;
                    toggleCCWRCS(true);
                    toggleCWRCS(false);
                }
            }
        }
        else
        {
            toggleCCWRCS(false);
            toggleCWRCS(false);
        }
    }

    public void setPDCSafety(bool flag)
    {
        pdcSafety = flag;
    }

    public void enqueueTargetQueue(Transform target)
    {
        targetQueue.Enqueue(target);
    }

    // recieves damage
    public void recieveDamage(float damage)
    {
        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            // lose target lock with player now that this ship is dead
            ps.setTargeted(false);

            // tell the tangible to destroy this object and the associated radar contact
            GameObject explosion = (GameObject)Instantiate(deathExplosion.gameObject, transform.position, transform.rotation);
            float delay = deathExplosion.main.startLifetime.constantMax;
            explosion.GetComponent<Rigidbody>().velocity = rb.velocity;
            GetComponent<Tangible>().destroy();
            Destroy(explosion, delay);
        }
    }

    public void toggleTPDSafety(bool flag)
    {
        tpdSafety = flag;
    }

    void OnTriggerEnter(Collider col)
    {
        // Whenever this ship is hit while not currently engaged (wandering), make it wander towards the direction that the projectile came from
        if (canDetectTarget(ps.transform) == false)
        {
            Vector3 displacement = ps.transform.position - transform.position;
            float randomScaler = Mathf.Pow(displacement.magnitude,0.5f); // sqaure root of displacement magnitude
            Vector3 randomPos = new Vector3(Random.Range(-randomScaler,randomScaler),Random.Range(-randomScaler,randomScaler),0f);
            // the wanderDestination should be somewhat near the player's position plus some uncertainty, depending on distance
            Vector3 destination = ps.transform.position + randomPos;

            if (col.GetComponent<Torpedo>())
            {
                Torpedo tpd = col.GetComponent<Torpedo>();

                if (tpd.getHarmsPlayer() == false) // if this torpedo came from the player
                {
                    wanderDestination = destination;
                }
            }
            else if (col.GetComponent<Bullet>())
            {
                Bullet bullet = col.GetComponent<Bullet>();

                if (bullet.getHarmsPlayer() == false) // if this bullet came from the player
                {
                    wanderDestination = destination;
                }
            }
        }
    }


}

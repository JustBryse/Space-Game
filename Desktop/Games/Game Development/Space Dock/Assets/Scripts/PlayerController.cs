using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameObject cwrcs; // all the clockwise rcs
    public GameObject ccwrcs; // all the counter clockwise rcs
    public ParticleSystem thrusterExhaust; // the thruster exhaust also has an audiosource
    public float maxSpeed = 90f;
    public float thrust = 10f;
    public float radialSpeed = 5f;

    Rigidbody rb; // the rigid body of this object
    UIManager uim;
    PlayerShip ps;

    List<Module> ccwrcsModules = new List<Module>();
    List<Module> cwrcsModules = new List<Module>();

    AudioSource thrusterAS;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        uim = UIManager.FindObjectOfType<UIManager>();
        ps = GetComponent<PlayerShip>();

        thrusterAS = thrusterExhaust.GetComponent<AudioSource>();

        initializeRCSModuleLists();
	}

    void initializeRCSModuleLists()
    {
        foreach (Transform rcs in ccwrcs.transform)
        {
            ccwrcsModules.Add(rcs.GetComponent<Module>());
        }

        foreach (Transform rcs in cwrcs.transform)
        {
            cwrcsModules.Add(rcs.GetComponent<Module>());
        }
    }

	// Update is called once per frame
	void Update () {
        controlShip();
	}

    private void controlShip()
    {
        uim = UIManager.FindObjectOfType<UIManager>();

        if (Input.GetKey(KeyCode.Q))
        {
            float activeCCWRCSCount = ccwrcsModules.Count;

            foreach (Module rcs in ccwrcsModules)
            {
                ParticleSystem rcsPS = rcs.GetComponent<ParticleSystem>();
                AudioSource rcsAS = rcs.GetComponent<AudioSource>();

                float volume = 1f / (float)Camera.main.orthographicSize;
                rcsAS.volume = volume;

                if (rcs.isOnline())
                {
                    if (rcsPS.isPlaying == false)
                    {
                        rcsPS.Play();
                    }

                    if (rcsAS.isPlaying == false)
                    {
                        rcsAS.Play();
                    }
                }
                else
                {
                    rcsPS.Stop();
                    rcsAS.Stop();
                    activeCCWRCSCount--;
                }
            }

            float radialSpeedModifer = activeCCWRCSCount / ccwrcsModules.Count; // a ratio of how many active rcs to total rcs in this angular direction

            Vector3 rotation = Vector3.forward * Time.deltaTime * radialSpeed * radialSpeedModifer;
            transform.Rotate(rotation);

            // deactivate the cwrcs
            foreach (Module rcs in cwrcsModules)
            {
                rcs.GetComponent<ParticleSystem>().Stop();
                rcs.GetComponent<AudioSource>().Stop();
            }
        }
        else if (Input.GetKey(KeyCode.E))
        {
            float activeCWRCSCount = cwrcsModules.Count;

            foreach (Module rcs in cwrcsModules)
            {
                ParticleSystem rcsPS = rcs.GetComponent<ParticleSystem>();
                AudioSource rcsAS = rcs.GetComponent<AudioSource>();

                float volume = 1f / (float)Camera.main.orthographicSize;
                rcsAS.volume = volume;

                if (rcs.isOnline())
                {
                    if (rcsPS.isPlaying == false)
                    {
                        rcsPS.Play();
                    }

                    if (rcsAS.isPlaying == false)
                    {
                        rcsAS.Play();
                    }
                }
                else
                {
                    rcsPS.Stop();
                    rcsAS.Stop();
                    activeCWRCSCount--;
                }
            }

            float radialSpeedModifer = activeCWRCSCount / cwrcsModules.Count; // a ratio of how many active rcs to total rcs in this angular direction

            Vector3 rotation = -Vector3.forward * Time.deltaTime * radialSpeed * radialSpeedModifer;
            transform.Rotate(rotation);

            // deactivate the ccwrcs
            foreach (Module rcs in ccwrcsModules)
            {
                rcs.GetComponent<ParticleSystem>().Stop();
                rcs.GetComponent<AudioSource>().Stop();
            }
        }
        else
        {
            // if the player isn't using any of the rcs then turn them all off
            foreach (Module rcs in ccwrcsModules)
            {
                rcs.GetComponent<ParticleSystem>().Stop();
                rcs.GetComponent<AudioSource>().Stop();
            }

            foreach (Module rcs in cwrcsModules)
            {
                rcs.GetComponent<ParticleSystem>().Stop();
                rcs.GetComponent<AudioSource>().Stop();
            }
        }

        if (Input.GetKey(KeyCode.Space) && thrusterExhaust.GetComponent<Module>().isOnline())
        {
            float volume = 1f / (float)Camera.main.orthographicSize;
            thrusterAS.volume = volume;

            if (thrusterExhaust.isPlaying == false)
            {
                thrusterExhaust.Play();
            }

            if (thrusterAS.isPlaying == false)
            {
                thrusterAS.Play();
            }

            Vector3 orientation = transform.eulerAngles + new Vector3(0f, 0f, 90f);
            Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * orientation.z), Mathf.Sin(Mathf.Deg2Rad * orientation.z), 0f).normalized;
            Vector3 force = direction * thrust * Time.deltaTime;
            rb.AddForce(force);

            ps.increaseRadarSig(ps.getThrusterRS(), false);
        }
        else
        {
            thrusterExhaust.Stop();
            thrusterAS.Stop();
        }

        // dont let velocity exceed a certain limit
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        string shipSpeedMessage = "Speed: " + ((int)rb.velocity.magnitude) + " m/s";
        uim.setShipInfoText(rb.velocity, transform);
        uim.setUIArrows(rb.velocity, transform.up);

        // toggles the sensor circle around the player ship that shows the range of the player ship's sensor emissions
        if(Input.GetKeyDown(KeyCode.C))
        {
            ps.toggleSensorSigCircle();
        }
    }

}

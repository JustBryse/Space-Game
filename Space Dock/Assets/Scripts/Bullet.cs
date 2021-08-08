using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public ParticleSystem bulletSpark;
    public AudioClip[] impactSounds;
    public AudioClip fireSound;
    public bool harmsPlayer = true;
    public float damage = 1f;
    [Range(0f, 1f)] public float critChance = 0.01f; // there is a very low chance that a bullet will knock out a module
    public float selfDestructDelay = 5f;

    GameObject spawner;
    Vector3 startVelocity;

    Rigidbody rb;
    PlayerShip ps;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
        //AudioSource.PlayClipAtPoint(fireSound, transform.position, 1f / (float)Camera.main.orthographicSize);
        Destroy(gameObject, selfDestructDelay);
	}

  void Update()
  {
    correctVelocity();
  }

    void OnTriggerEnter(Collider col)
    {
        if (harmsPlayer)
        {
            if (col.GetComponent<PlayerShip>())
            {
                ps.receiveDamage(damage, critChance);
                destroy(ps.GetComponent<Rigidbody>().velocity);
            }
            else if (col.GetComponent<Torpedo>())
            {
                Torpedo tpd = col.GetComponent<Torpedo>();
                if (tpd.getHarmsPlayer() == false) // only destroy torpedos that are friendly to the player because this bullet is meant to harm the player
                {
                    tpd.recieveDamage(damage);
                    destroy(tpd.GetComponent<Rigidbody>().velocity);
                }
            }
        }
        else
        {
            if (col.GetComponent<AIController>())
            {
                col.GetComponent<AIController>().recieveDamage(damage);
                destroy(col.GetComponent<Rigidbody>().velocity);
            }
            else if (col.GetComponent<Torpedo>())
            {
                Torpedo tpd = col.GetComponent<Torpedo>();
                if (tpd.getHarmsPlayer()) // destroy torpedos that would harm the player since this bullet would not harm the player
                {
                    tpd.recieveDamage(damage);
                    destroy(tpd.GetComponent<Rigidbody>().velocity);
                }
            }
        }
    }

    void destroy(Vector3 velocity)
    {
        /*
        AudioClip impactSound = impactSounds[Random.Range(0, impactSounds.Length - 1)];
        float volume = 1f / (float)Camera.main.orthographicSize;
        AudioSource.PlayClipAtPoint(impactSound, transform.position, volume);
        */

        GameObject spark = (GameObject)Instantiate(bulletSpark.gameObject, transform.position, Quaternion.identity);
        spark.GetComponent<Rigidbody>().velocity = velocity;
        float delay = spark.GetComponent<ParticleSystem>().main.startLifetime.constant;
        Destroy(spark, delay);
        Destroy(gameObject);
    }

    void correctVelocity()
    {
      if(startVelocity != null)
      {
        rb.velocity = startVelocity;
      }
    }

    // recieves the gameObject that spawned this bullet
    public void setSpawner(GameObject spawner)
    {
        this.spawner = spawner;
    }

    public bool getHarmsPlayer()
    {
        return harmsPlayer;
    }

    public void setStartVelocity(Vector3 startVelocity)
    {
        this.startVelocity = startVelocity;
    }

}

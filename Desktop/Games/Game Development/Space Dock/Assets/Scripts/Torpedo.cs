using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour {

    public static int playerTPDLayer = 9; // I dont like hard coding the layer to the value of 9 but the LayerMask class has failed me or I dont understand it
    public TorpedoExplosion torpExplosion;
    public AudioClip explosionSound;
    public AudioClip launchSound;
    
    public bool harmsPlayer = false;
    public float startHitPoints = 10f;
    public float damage = 100f;
    [Range(0f, 1f)] public float critChance = 0.95f; // there is a high chance that a torpedo will knock out a module
    public float thrust = 30f;
    public float rotationSpeed = 10f;
    public float topSpeed = 100f;
    public float drag = 1f;

    float hitPoints;

    Transform target;
    Rigidbody rb;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        hitPoints = startHitPoints;
        //AudioSource.PlayClipAtPoint(launchSound, transform.position, 1f / (float) Camera.main.orthographicSize);
    }

    // Update is called once per frame
    void Update() {
        alignToTarget();
        pursueTarget();
    }

    void alignToTarget()
    {
        if (target == null)
        {
            destroy();
            return;
        }

        if (canDetectTarget(target))
        {
            Vector3 direction = (target.position - transform.position).normalized;

            float directionAngle = Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg; // direction angle in degrees

            if (target.position.x < transform.position.x)
            {
                directionAngle += 90f;
            }
            else
            {
                directionAngle -= 90f;
            }

            transform.eulerAngles = new Vector3(0f, 0f, directionAngle);
        }
    }

    void pursueTarget()
    {
        if (target == null)
        {
            destroy();
            return;
        }

        if (canDetectTarget(target))
        {
            // make sure this torpedo has drag while pursuing the target
            if (rb.drag == 0)
            {
                rb.drag = drag;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 force = direction * thrust * Time.deltaTime;
            rb.AddForce(force);

            if (rb.velocity.magnitude > topSpeed)
            {
                rb.velocity = rb.velocity.normalized * topSpeed;
            }
        }
        else
        {
            // allow the torpedo do drift if it cannot detect its target
            rb.drag = 0;
        }
    }

    public void setTarget(Transform target)
    {
        this.target = target;
    }

    void destroy()
    {
        GetComponent<Tangible>().destroy();
    }

    // used for being destroyed by a collision with a ship
    public void destroy(Vector3 velocity)
    {
        //float volume = 1f / (float)Camera.main.orthographicSize;
        //AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);

        GameObject explosion = (GameObject)Instantiate(torpExplosion.gameObject, transform.position, Quaternion.identity);
        explosion.GetComponent<Rigidbody>().velocity = velocity;

        float delay = explosion.GetComponent<ParticleSystem>().main.startLifetime.constant;

        Destroy(explosion, delay);

        GetComponent<Tangible>().destroy();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Transform>() == target)
        {
            if (col.GetComponent<PlayerShip>())
            {
                col.GetComponent<PlayerShip>().receiveDamage(damage, critChance);
            }
            else if (col.GetComponent<AIController>())
            {
                col.GetComponent<AIController>().recieveDamage(damage);
            }
     
            // drops the torpedo explosion
            destroy(col.GetComponent<Rigidbody>().velocity);
        }
    }

    public bool getHarmsPlayer()
    {
        return harmsPlayer;
    }

    public float getHitPoints()
    {
        return hitPoints;
    }

    public void recieveDamage(float damage)
    {
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            destroy();
        }
    }

    bool canDetectTarget(Transform target)
    {
        int targetLayer = 0;
        if (harmsPlayer)
        {
            // if this torpedo is pursuing the player then scan for the player
            targetLayer = LayerMask.GetMask("PlayerStuff");
        }
        else
        {
            // if this torpedo is sent from the player then scan the for AI controlled stuff
            targetLayer = LayerMask.GetMask("AICollider");
        }

        RaycastHit hit;
        Vector3 dir = (target.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, targetLayer))
        {
            if (harmsPlayer)
            {
                if (hit.collider.GetComponent<PlayerShip>())
                {
                    return true;
                }
            }
            else
            {
                if (hit.collider.GetComponent<AIController>() || hit.collider.GetComponent<Torpedo>())
                {
                    return true;
                }
            }

            return false; // return false if line of sight with the target cannot be made
        }
        else
        {
            return false; // return false if the raycast failed
        }
    }

}

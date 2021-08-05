using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoExplosion : MonoBehaviour
{
    public bool harmsPlayer = false;
    public float colliderDeathDelay = 0.125f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(destroyCollider());
    }

    IEnumerator destroyCollider()
    {
        yield return new WaitForSeconds(colliderDeathDelay);
        Destroy(GetComponent<SphereCollider>());
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Torpedo>())
        {
            Torpedo tpd = col.GetComponent<Torpedo>();

            if (tpd.getHarmsPlayer() && harmsPlayer == false)
            {
                tpd.destroy(rb.velocity);
            }
            else if (tpd.getHarmsPlayer() == false && harmsPlayer)
            {
                tpd.destroy(rb.velocity);
            }
        }
    }
}

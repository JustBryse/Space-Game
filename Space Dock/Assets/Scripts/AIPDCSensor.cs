using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPDCSensor : MonoBehaviour {

    public AIController aic;

    // Use this for initialization
    void Start()
    {
        GetComponent<SphereCollider>().radius = aic.pdcRange;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<PlayerShip>())
        {
            aic.enqueueTargetQueue(col.transform);
        }
        else if (col.GetComponent<Torpedo>())
        {
            if (col.GetComponent<Torpedo>().getHarmsPlayer() == false)
            {
                aic.enqueueTargetQueue(col.transform);
            }
        }
    }

    /*
    void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<Torpedo>())
        {
            // send a message to pdc control that a torpedo has exited pdc range. If that torpedo is the current pdc target than fire control must forget about it
            aic.clearCurrentTarget(col.transform);
        }
    }
    */
}

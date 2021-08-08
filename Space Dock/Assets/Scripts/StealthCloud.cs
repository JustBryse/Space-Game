using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthCloud : MonoBehaviour {

    UIManager uim;
    PlayerShip ps;

    public float psStealthFade = 0.25f;
    public float radarSigInCloud = 250f;

    // Use this for initialization
    void Start()
    {
        uim = UIManager.FindObjectOfType<UIManager>();
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
    }

    // the stealth cloud scrambles the players radar
    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<PlayerShip>())
        {
            uim.toggleRadar(false);
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            Color srColor = sr.color;
            srColor = new Color(srColor.r, srColor.g, srColor.b, psStealthFade);
            sr.color = srColor;

            ps.setMinRadarSig(radarSigInCloud);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<PlayerShip>())
        {
            if (ps.getSensorModule().isOnline())
            {
                uim.toggleRadar(true);
            }
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            Color srColor = sr.color;
            srColor = new Color(srColor.r, srColor.g, srColor.b, 1f);
            sr.color = srColor;

            float startRadarSig = ps.getStartRadarSig();
            ps.setMinRadarSig(startRadarSig);

            // if the radar sig is smaller than the start sig when leaving the cloud, then increase the radar sig to its start value
            // this is to simulate losing the emission shielding that the cloud provides upon leaving the cloud
            if (ps.getRadarSig() < startRadarSig) 
            {
                ps.setRadarSig(startRadarSig);
            }
        }
    }
}

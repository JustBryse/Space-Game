using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBackground : MonoBehaviour {

    PlayerShip ps;

    void Start()
    {
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
    }

    // Update is called once per frame
    void Update() {
        followPlayerShip();
    }

    void followPlayerShip()
    {
        Vector3 newPos = new Vector3(ps.transform.position.x, ps.transform.position.y, 5f);
        transform.position = newPos;
    }
}

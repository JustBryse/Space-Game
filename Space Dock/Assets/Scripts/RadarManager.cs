using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// panel is 856w x 536h

public class RadarManager : MonoBehaviour {

    private const float maxScale = 65536f;
    private const float minScale = 1f;

    private float currentScale;

	// Use this for initialization
	void Start () {
        currentScale = minScale;
	}
	
	// Update is called once per frame
	void Update () {
        adjustZoom();
	}

    private void adjustZoom()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt)  && currentScale < maxScale)
        {
            currentScale *= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && currentScale > minScale)
        {
            currentScale /= 2;
        }
    }

    public float getRadarScale()
    {
        return currentScale;
    }
}

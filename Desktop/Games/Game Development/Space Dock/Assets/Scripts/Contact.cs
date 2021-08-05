using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Contact : MonoBehaviour {

    public static Contact currectContact; // the contact that has been most recently clicked by the user

    RectTransform rt;
    Tangible tangible; // the real object that this contact represents on the map
    RadarManager rm;
    RectTransform rmrt; // the rect transform of the radar manager
    PlayerController pc; // the player
    UIManager uim;

    public Text idText;
    public Text telemtryText;
    public Image contactImage;
    public Image contactArrow;

    // Use this for initialization
    void Start() {
        rt = GetComponent<RectTransform>();
        uim = UIManager.FindObjectOfType<UIManager>();
        setUIScale();
    }

    // Update is called once per frame
    void Update() {
        updatePosition();
        updateTelemetryText();
        updateVisibility();
        setArrowDir();

        // keep updating the UIManager with contact info from this contact if it is in focus by the player
        if (currectContact == this)
        {
            uim.setContactPanel(tangible);
        }
    }

    void setUIScale()
    {
        rt.localScale = new Vector3(1, 1, 0);
    }

    private void updatePosition()
    {
        Vector3 tangPos = tangible.GetComponent<Transform>().position;
        Vector3 pcPos = pc.transform.position;
        Vector3 difference = tangPos - pcPos;

        rt.localPosition = difference / rm.getRadarScale();
    }

    private void updateTelemetryText()
    {
        int spdDifference = (int)(pc.GetComponent<Rigidbody>().velocity - tangible.GetComponent<Rigidbody>().velocity).magnitude;
        Vector3 tangPos = tangible.transform.position;
        Vector3 pcPos = pc.transform.position;
        int posDifference = (int)(pcPos - tangPos).magnitude;
        string message = posDifference + " m - " + spdDifference + " m/s";
        telemtryText.text = message;
    }

    void setArrowDir()
    {
        Vector3 tangDir = tangible.GetComponent<Rigidbody>().velocity.normalized;
        contactArrow.GetComponent<RectTransform>().up = -tangDir;
    }

    private void updateVisibility()
    {
        Vector3 pos = rt.localPosition;
        float rmrtWidth = rmrt.rect.width;
        float rmrtHeight = rmrt.rect.height;

        //print(pos);
        if (pos.x > rmrtWidth / 2 || pos.x < -rmrtWidth / 2 || pos.y > rmrtHeight / 2 || pos.y < -rmrtHeight / 2)
        {
            idText.enabled = false;
            telemtryText.enabled = false;
            contactImage.enabled = false;
            contactArrow.enabled = false;
        }
        else
        {
            idText.enabled = true;
            telemtryText.enabled = true;
            contactImage.enabled = true;
            contactArrow.enabled = true;
        }
    }

    // receives the tangible object in the game space that this contact will represent on the map.
    public void initialize(Tangible tangible)
    {
        this.tangible = tangible;
        rt = GetComponent<RectTransform>();
        pc = PlayerController.FindObjectOfType<PlayerController>();
        rm = RadarManager.FindObjectOfType<RadarManager>();
        rmrt = rm.GetComponent<RectTransform>();
        rt.SetParent(rmrt);

        idText.text = tangible.getID();
    }

    // this function is called by this class during an onClick event from the button on this object
    public void setContactPanel()
    {
        currectContact = this;
        uim.setContactPanel(tangible);
        uim.openContactPanel();
    }

    public Tangible getTangible()
    {
        return tangible;
    }

    public void destroy()
    {
        if (currectContact == this)
        {
            currectContact = null;
            uim.closeContactPanel();
        }

        Destroy(gameObject);
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject sensorLockedPanel;
    public Image pausePanel;
    public Slider playerHitPointsSlider;
    public Button pdcReloadButton;
    public Button tpdReloadButton;
    public Slider tpdMagSlider;
    public Text tpdMagText;
    public Slider pdcMagSlider;
    public Text pdcMagText;
    public Image velocityArrow;
    public Image orientationArrow;
    public Image radarOfflineBackground;
    public Button repairButton;
    public Text repairButtonText;
    public Text moduleText;
    public Image mapCameraBox;
    public Text mapZoomText;
    public Button activatePDCButton;
    public Button deactivatePDCButton;
    public Text fireTorpedoText;
    public GameObject contactPanel;
    public Text contactText;
    public GameObject crewmanCard;
    public Image crewmanCardPortrait;
    public Text crewmanCardText;
    public Text shipSpeedText;
    public RadarManager rm;
    public int maxCamScale = 512;
    public int minCamScale = 16;

    RectTransform mapCamBoxRT; // the rectTransform of the mapCameraBox
    RectTransform rmrt; // recttransform of radar manager
    PlayerShip ps;
    Transform inViewTransform; // the gameObject that the camera will follow.

    int currentCamScale; // the current camera zoom
    Vector2 startMapAnchoredPos; // stores the initial anchored position of the radar screen

    int crewIndex = 0; // stores the current crewman index that is displayed in the UI from the PlayerShip's crew list

    // Use this for initialization
    void Start()
    {
        mapCamBoxRT = mapCameraBox.GetComponent<RectTransform>();
        rmrt = rm.GetComponent<RectTransform>();
        ps = PlayerShip.FindObjectOfType<PlayerShip>();

        inViewTransform = ps.transform;

        currentCamScale = minCamScale;

        startMapAnchoredPos = rmrt.anchoredPosition;
    }

    // Update is called once per frame
    void Update() {
        toggleRadarScale();
        adjustCameraZoom();
        manuallyMoveCamera();
        moveCameraToInViewTransform();
    }

    void manuallyMoveCamera()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 camPos = Camera.main.transform.position;
            camPos.z = 0;
            Vector3 direction = mousePos - camPos + new Vector3(0f, 0f, -10f);
            Camera.main.transform.Translate(direction * Time.deltaTime);

            Vector3 relativePos = camPos - ps.transform.position;
            mapCamBoxRT.localPosition = relativePos / rm.getRadarScale();


            Vector3 pos = mapCamBoxRT.localPosition;
            float rmrtWidth = rmrt.rect.width;
            float rmrtHeight = rmrt.rect.height;

            //print(pos);
            if (pos.x > (rmrtWidth / 2)  - (mapCamBoxRT.rect.width / 2) || pos.x < (-rmrtWidth / 2) + (mapCamBoxRT.rect.width / 2) || pos.y > (rmrtHeight / 2) - (mapCamBoxRT.rect.height / 2) || pos.y < (-rmrtHeight / 2) + (mapCamBoxRT.rect.height /2))
            {
                mapCameraBox.enabled = false;
            }
            else
            {
                mapCameraBox.enabled = true;
            }
        }
        else
        {
            // reset the camera box to the center of the map when the player isn't moving it
            mapCamBoxRT.localPosition = Vector3.zero;
            mapCameraBox.enabled = true;
        }
    }

    // sets the inViewTransform to the object who's contact the user clicked last
    public void setInViewTransformAsCurrentContact()
    {
        inViewTransform = Contact.currectContact.getTangible().transform;
    }

    public void setInViewTransformAsPlayer()
    {
        inViewTransform = ps.transform;
    }

    void moveCameraToInViewTransform()
    {
        // only move the camera if the inViewTransform is not destroyed
        if (inViewTransform)
        {
            // have the camera follow the player if the player is not manually moving the camera
            if (Input.GetMouseButton(1) == false)
            {
                Camera.main.transform.position = new Vector3(inViewTransform.position.x, inViewTransform.position.y, -10f);
            }
        }
    }

    // minimizes and expands the radar screen
    private void toggleRadarScale()
    {
        // NOTE: radar panel is anchored to the bottom right corner
        /*
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector2 rightAnchoredOrigin = new Vector2(-Screen.width / 2f, Screen.height / 2f);
            Image rmImage = rm.GetComponent<Image>();

            if (rmrt.anchoredPosition == startMapAnchoredPos)
            {
                rmrt.anchoredPosition = rightAnchoredOrigin;
                rmrt.localScale *= 3f;
                rmImage.color = new Color(rmImage.color.r, rmImage.color.g, rmImage.color.b, 0.125f);
            }
            else if (rmrt.anchoredPosition == rightAnchoredOrigin)
            {
                rmrt.anchoredPosition = startMapAnchoredPos;
                rmrt.localScale /= 3f;
                rmImage.color = new Color(rmImage.color.r, rmImage.color.g, rmImage.color.b, 1f);
            }
        }
        */
        mapZoomText.text = ">" + rm.getRadarScale() + "x";
    }

    private void adjustCameraZoom()
    {
        if (Input.mouseScrollDelta.y < 0 && currentCamScale < maxCamScale)
        {
            currentCamScale *= 2;
            Camera.main.orthographicSize = currentCamScale;

        }
        else if (Input.mouseScrollDelta.y > 0 && currentCamScale > minCamScale)
        {
            currentCamScale /= 2;
            Camera.main.orthographicSize = currentCamScale;
        }
    }

    public void setShipInfoText(Vector3 velocity, Transform shipTransform)
    {
        float speed = Mathf.Round(velocity.magnitude);
        float direction = Vector3.Angle(velocity, Vector3.right);
        float orientation = Vector3.Angle(shipTransform.up, Vector3.right);

        if (velocity.y < 0)
        {
            direction = 360f - direction;
        }

        if (shipTransform.up.y < 0)
        {
            orientation = 360f - orientation;
        }

        //string targetLocked = "No";

        if (ps.isTargetLocked())
        {
            //targetLocked = "Yes";
            sensorLockedPanel.SetActive(true);
        }
        else
        {
            sensorLockedPanel.SetActive(false);
        }

        float hitPointPercentage = Mathf.Round(ps.getHitpoints() / ps.getMaxHitPoints() * 100f);

        shipSpeedText.text =
        ">Speed: " + speed + " m/s" + "\n"
        + ">Heading: " + Mathf.Round(direction) + "\n"
        + ">Orientation: " + Mathf.Round(orientation) + "\n"
        + ">Hull Armour: " + hitPointPercentage + "%" + "\n"
        + ">Radar Sig: " + Mathf.Round(ps.getRadarSig()) + " m"; //+ "\n"
        //+ ">Target Locked: " + targetLocked;
    }

    public void toggleCrewmanCard()
    {
        if (crewmanCard.activeInHierarchy == false)
        {
            crewmanCard.SetActive(true);

            List<Crewman> crew = ps.getCrew();
            Crewman crewman = crew[crewIndex];
            setCrewmanCard(crewman);
        }
        else
        {
            crewmanCard.SetActive(false);
        }
    }

    private void setCrewmanCard(Crewman crewman)
    {
        crewmanCardPortrait.sprite = crewman.getPortrait();
        string crewmanInfo =
        ">Name: " + crewman.getName() + "\n" +
        ">Profession: " + crewman.getProfession().ToString() + "\n" +
        ">Intellect: " + crewman.getIntellect().getLevel() + "\n" +
        ">Coordination: " + crewman.getCoordination().getLevel() + "\n" +
        ">Brawn: " + crewman.getBrawn().getLevel() + "\n" +
        ">Temperment: " + crewman.getTemperment().getLevel() + "\n" +
        ">Productivity: " + crewman.getProductivity().getLevel();
        crewmanCardText.text = crewmanInfo;
    }

    public void nextCrewmanCard()
    {
        List<Crewman> crew = ps.getCrew();

        if (crewIndex < crew.Count - 1)
        {
            crewIndex++;
        }
        else
        {
            crewIndex = 0;
        }

        Crewman crewman = crew[crewIndex];
        setCrewmanCard(crewman);
    }

    public void previousCrewmanCard()
    {
        List<Crewman> crew = ps.getCrew();

        if (crewIndex > 0)
        {
            crewIndex--;
        }

        {
            crewIndex = crew.Count - 1;
        }

        Crewman crewman = crew[crewIndex];
        setCrewmanCard(crewman);
    }

    // called from a contact object when the player clicks on a contact UI button element
    public void setContactPanel(Tangible tang)
    {
        string message =
        ">ID: " + tang.getID() + "\n" +
        ">Relative Speed: " + (int)(ps.GetComponent<Rigidbody>().velocity - tang.GetComponent<Rigidbody>().velocity).magnitude + " m/s" + "\n" +
        ">Distance: " + (int)(ps.transform.position - tang.transform.position).magnitude + " m";

        contactText.text = message;
    }

    public void openContactPanel()
    {
        contactPanel.SetActive(true);
    }

    public void closeContactPanel()
    {
        contactPanel.SetActive(false);
    }

    // called from the fire torpedo button
    public void fireTorpedo()
    {
        ps.fireTorpedo(Contact.currectContact.getTangible());
    }

    public void setFireTorpedoText(string message)
    {
        fireTorpedoText.text = message;
    }

    public void activatePDC()
    {
        ps.setPDCSafety(false);
        activatePDCButton.interactable = false;
        deactivatePDCButton.interactable = true;
    }

    public void deactivatePDC()
    {
        ps.setPDCSafety(true);
        activatePDCButton.interactable = true;
        deactivatePDCButton.interactable = false;
    }

    // UIModules tell the UIManager to update the UIText
    public void setModuleText()
    {
        if (UIModule.current)
        {
            Module realModule = UIModule.current.getRealModule();

            // make sure the repair button's text makes sense depending on the status of the module
            setRepairButtonText();

            string message =
            ">Module: " + realModule.getUIName() + "\n" +
            ">Status: " + realModule.getStatus() + "\n";

            if (realModule.isOnline() == false)
            {
                float repairProgress = Mathf.Round(realModule.getRepairProgress() * 100f);

                message += ">Repair Progress: " + repairProgress + "%" + "\n";

                if (realModule.isRepairing() == true)
                {
                    message += ">Repairing \n";
                }
                else
                {
                    message += ">Not Repairing \n";
                }
            }

            moduleText.text = message;
        }
    }

    void setRepairButtonText()
    {
        Module realModule = UIModule.current.getRealModule();

        if (realModule.isOnline() == false && realModule.isRepairing() == true)
        {
            repairButtonText.text = "Pause";
        }
        else
        {
            repairButtonText.text = "Repair";
        }
    }

    public void toggleRepairs()
    {
        Module currentModule = UIModule.current.getRealModule();

        // only allow repair toggling is the module is actually damaged and this offline
        if (currentModule.isOnline() == false)
        {
            if (currentModule.isRepairing() == false)
            {
                currentModule.startRepairs();
                repairButtonText.text = "Start";
            }
            else
            {
                currentModule.pauseRepairs();
                repairButtonText.text = "Pause";
            }
        }
    }

    // called from real module
    public void setUIModuleColor(UIModule uiModule)
    {
        Image uiModImage = uiModule.GetComponent<Image>();
        if (uiModule.getRealModule().isOnline())
        {
            uiModImage.color = new Color(0f, 1f, 0f, 0.25f);
        }
        else
        {
            uiModImage.color = new Color(1f, 0f, 0f, 0.25f);
        }
    }

    public void toggleRadar(bool flag)
    {
        if (flag)
        {
            //rm.gameObject.SetActive(true);
            radarOfflineBackground.gameObject.SetActive(false);
        }
        else
        {
            //rm.gameObject.SetActive(false);
            radarOfflineBackground.gameObject.SetActive(true);
            closeContactPanel();
        }
    }

    public void setUIArrows(Vector3 velocity, Vector3 orientation)
    {
        RectTransform velArrowRT = velocityArrow.GetComponent<RectTransform>();
        velArrowRT.up = velocity.normalized;
        Vector3 velArrowPos = velArrowRT.up * (velArrowRT.rect.height / 2f);
        velArrowRT.localPosition = velArrowPos;

        RectTransform oriArrowRT = orientationArrow.GetComponent<RectTransform>();
        oriArrowRT.up = orientation;
        Vector3 oriArrowPos = oriArrowRT.up * (oriArrowRT.rect.height / 2f);
        oriArrowRT.localPosition = oriArrowPos;
    }

    // Tells the player ship to load a single torpedo into the torpedo mag
    public void startTorpedoReload()
    {
        if (ps.canReloadTorpedoMag())
        {
            tpdMagText.text = ">Loading Torpedo";
            StartCoroutine(ps.reloadTorpedoMag());
            tpdReloadButton.interactable = false;
        }
    }

    public void finishTorpedoReload()
    {
        tpdReloadButton.interactable = true;
    }

    // tells the player ship to reload the shared pdc magazine
    public void startPDCReload()
    {
        if (ps.canReloadPDCMag())
        {
            pdcMagText.text = ">Reloading PDC Magazine";
            deactivatePDC();
            StartCoroutine(ps.reloadPDCMag());
            pdcReloadButton.interactable = false; // dont let the player reload while reloaded

            // also deactivate the pdc toggle buttons because they can change the pdcSafety flag in the middle of a reload. This would be bad
            activatePDCButton.interactable = false;
            deactivatePDCButton.interactable = false;
        }
    }

    // call from the player ship to inform the UIM that the reload button can be enabled again
    public void finishPDCReload()
    {
        pdcReloadButton.interactable = true;
        activatePDCButton.interactable = true; // allow the player to turn on the pdcs after the reload is complete
    }

    // updates the slider position to match the torpedo magazine
    public void setTorpedoMagSlider(float maxMagAmmo, float currentMagAmmo)
    {
        // convert mag ammo count into a ratio between 0 and 1 to send to the slider
        tpdMagSlider.value = (currentMagAmmo / maxMagAmmo);
        tpdMagText.text = ">TPD Magazine: " + currentMagAmmo;
    }

    // updates the slider position to match the pdc magazine
    public void setPDCMagSlider(float maxMagAmmo, float currentMagAmmo)
    {
        // convert mag ammo count into a ratio between 0 and 1 to send to the slider
        pdcMagSlider.value = (currentMagAmmo / maxMagAmmo);
        pdcMagText.text = ">PDC Magazine: " + currentMagAmmo;
    }

    public void setPlayerHitPointsSlider(float currentHP, float maxHP)
    {
        float ratio = currentHP / maxHP; // the slider accepts a value between zero and one
        playerHitPointsSlider.value = ratio;
    }

    // called when the player clicks down on the firePDCPanel while pdcs are enabled
    public void pullPDCTrigger()
    {
        if (!Input.GetMouseButtonDown(1))
        {
            ps.setPDCTrigger(true);
        }
    }

    public void releasePDCTrigger()
    {
        if (!Input.GetMouseButtonDown(1))
        {
            ps.setPDCTrigger(false);
        }
    }

    // activate pause panel
    public void pause()
    {
        pausePanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void resume()
    {
        Time.timeScale = 1f;
        pausePanel.gameObject.SetActive(false);
    }
}

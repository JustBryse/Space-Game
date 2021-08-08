using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tangible : MonoBehaviour {

    public Contact contactPrefab;
    public string id;
    //public float size;
    //public float irSig;
    //public float albedo;
    //public float radarSig;

    GameObject activeContact;

    PlayerShip ps;
    UIManager uim;

    // Use this for initialization
    void Start() {
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
        uim = UIManager.FindObjectOfType<UIManager>();
        createContact();
    }

    // Update is called once per frame
    void Update() {
        setContactVisibility();
    }

    void setContactVisibility()
    {
        bool visible = ps.canDetectTarget(transform);

        // if the current;y selected contact is no longer detectable by the player than close the contact panel
        if (Contact.currectContact == activeContact.GetComponent<Contact>() && visible == false)
        {
            uim.closeContactPanel();
        }

        activeContact.SetActive(visible);
    }
   
    // creates a contact to go on the mini map
    void createContact()
    {
        activeContact = (GameObject)Instantiate(contactPrefab.gameObject, transform.position, Quaternion.identity);
        activeContact.SetActive(true);

        Contact contact = activeContact.GetComponent<Contact>();
        contact.initialize(this);
    }

    public void setID(string id)
    {
        this.id = id;
    }

    public string getID()
    {
        return id;
    }

    // called automatically when this object is destroyed
    public void destroy()
    {
        activeContact.GetComponent<Contact>().destroy();
        Destroy(gameObject);
    }
    
}

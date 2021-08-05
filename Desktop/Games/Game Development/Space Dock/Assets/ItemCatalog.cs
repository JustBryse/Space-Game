using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCatalog : MonoBehaviour {

    public Image spawnPosPanel;
    public Dropdown catalogDropdown;
    public Text currentItemText;

    void Start()
    {
        GameObject.DontDestroyOnLoad(spawnPosPanel.gameObject);
    }
    
    public List<ItemIcon> itemIcons;

    public void addItem()
    {
        int value = catalogDropdown.value;
        ItemIcon itemIcon = itemIcons[value];
        GameObject newItemIcon = (GameObject)Instantiate(itemIcon.gameObject, Vector3.zero, Quaternion.identity);
        RectTransform rt = newItemIcon.GetComponent<RectTransform>();
        rt.parent = spawnPosPanel.GetComponent<RectTransform>();
        rt.localPosition = Vector2.zero;
    }

    public void removeItem()
    {
        if (ItemIcon.currentItemIcon && ItemIcon.currentItemIcon.itemId.Equals("Player Ship") == false)
        {
            setCurrentItemText("Current Item: None");
            Destroy(ItemIcon.currentItemIcon.gameObject);
        }
    }

    public void setCurrentItemText(string text)
    {
        currentItemText.text = text;
    }
}

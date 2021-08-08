using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIcon : MonoBehaviour {

    public static ItemIcon currentItemIcon;
    public GameObject itemPrefab;
    public string itemId;

    RectTransform rt;
    ItemCatalog ic;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        ic = ItemCatalog.FindObjectOfType<ItemCatalog>();
        //GameObject.DontDestroyOnLoad(gameObject);
    }

    // this is a UI object that is anchored at the center
    Vector3 getItemSpawnPos()
    {
        RectTransform spawnPosPanelRT = GameObject.Find("SpawnPositionPanel").GetComponent<RectTransform>();
        float width = spawnPosPanelRT.rect.width;
        float height = spawnPosPanelRT.rect.height;

        float xRatio = rt.rect.x / width;
        float yRatio = rt.rect.y / height;

        Vector3 worldPos = new Vector3(3000f * xRatio, 3000f * yRatio, 0f);
        return worldPos;
    }

    public void spawnItem()
    {
        Vector3 spawnPos = getItemSpawnPos();
        GameObject item = (GameObject) Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        GameObject.DontDestroyOnLoad(item);
        Destroy(gameObject);
    }

    public void followMouse()
    {
        currentItemIcon = this;
        ic.setCurrentItemText("Current Item: " + itemId);
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        rt.position = mousePos;
    }
}

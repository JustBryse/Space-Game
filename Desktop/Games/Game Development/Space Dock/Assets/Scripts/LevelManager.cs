using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public void loadStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Start");
        // go back to start
    }

    public void loadGame()
    {
        spawnItems();
        SceneManager.LoadScene("Game");
    }

    void spawnItems()
    {
        ItemIcon[] itemIcons = ItemIcon.FindObjectsOfType<ItemIcon>();

        foreach (ItemIcon ii in itemIcons)
        {
            ii.spawnItem();
        }
    }
}

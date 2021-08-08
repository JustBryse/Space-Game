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

    public void loadGame(int levelNumber)
    {
        string level = "Game " + levelNumber.ToString();
        SceneManager.LoadScene(level);
    }

    public void exitApplication()
    {
      Application.Quit();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public Dropdown dropdown;
    public LevelManager lm;

    // load the level based on the value of the dropdown
    public void loadLevel()
    {
      int levelNumber = dropdown.value + 1;
      lm.loadGame(levelNumber);
    }

}

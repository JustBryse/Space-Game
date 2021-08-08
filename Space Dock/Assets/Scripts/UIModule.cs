using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModule : MonoBehaviour {

    public static UIModule current; // the most recently selected UIModule

    Module realModule; // the real module that this ui element represents
    UIManager uim;

	// Use this for initialization
	void Start () {
        uim = UIManager.FindObjectOfType<UIManager>();
        initialize();
	}

    void initialize()
    {
        Module[] allModules = Module.FindObjectsOfType<Module>();
        foreach (Module mod in allModules)
        {
            if (mod.name.Equals(name))
            {
                realModule = mod;
                realModule.setUIModule(this);
            }
        }

        /*
        if(realModule == null)
        {
            print("No module found for " + name + "UI element");
        }
        */
    }

    public Module getRealModule()
    {
        return realModule;
    }

    // this is called by the button attached to this class' UI object
    public void requestUIManagerSetModuleText()
    {
        current = this;
        uim.setModuleText();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Module : MonoBehaviour {

    public enum ModuleType {Drive, CCWRCS, CWRCS, TorpedoTube, PDC, RailGun, Sensor}
    public ModuleType moduleType;

    public string uiName;
    public float repairTime = 10f; // time in seconds that it takes to repair from 0% to 100%

    bool online = true;
    [Range(0f, 1f)] float repairProgress = 0; // 0% to 100%
    bool repairing = false;
    static List<Module> repairingModules = new List<Module>(); // stores all the modules that are currenly being repaired

    UIManager uim;
    UIModule uiModule; // the UI representation of this module
    PlayerShip ps;

    void Start()
    {
        uim = UIManager.FindObjectOfType<UIManager>();
        ps = PlayerShip.FindObjectOfType<PlayerShip>();
    }

    void Update()
    {
        repair();
    }

    void repair()
    {
        if (uiModule == UIModule.current)
        {
            uim.setModuleText();
        }

        if (repairing && repairProgress < 1)
        {
            // keep repairing
            repairProgress += (Time.deltaTime / repairTime);
        }
        else if (repairProgress >= 1)
        {
            // stop repairing because the module is fixed
            finishRepairs();
        }
    }

    public void setUIModule(UIModule uiModule)
    {
        this.uiModule = uiModule;
    }

    public bool isRepairing()
    {
        return repairing;
    }

    public float getRepairProgress()
    {
        return repairProgress;
    }

    public bool isOnline()
    {
        return online;
    }

    public string getStatus()
    {
        if (online)
        {
            return "Online";
        }
        else
        {
            return "Offline";
        }
    }

    // critChance should ideally be a value between 0 and 1 so as to not guarantee a critical hit, but rather increase the odds only
    public void recieveDamage(float critChance)
    {
        if (Random.value < critChance)
        {
            online = false;
            uim.setUIModuleColor(uiModule);

            // no ship will have more than one sensor module, so if a sensor module is taken down then so is the radar screen
            if (moduleType == ModuleType.Sensor)
            {
                uim.toggleRadar(false);
            }
        }
    }

    // start repairs
    public void startRepairs()
    {
        if (repairingModules.Count < ps.getMaxModuleRepairCount())
        {
            repairing = true;
            repairingModules.Add(this);
        }

        //print("repairingModulesCount = " + repairingModules.Count);
    }

    // pause the repairs of this module but don't reset repair progress
    public void pauseRepairs()
    {
        repairing = false;
        repairingModules.Remove(this);
    }

    // reset everything and return module to active status
    void finishRepairs()
    {
        repairing = false;
        repairingModules.Remove(this);
        online = true;
        repairProgress = 0;
        uim.setUIModuleColor(uiModule);

        if (moduleType == ModuleType.Sensor)
        {
            uim.toggleRadar(true);
        }
    }

    public string getUIName()
    {
        return uiName;
    }
}

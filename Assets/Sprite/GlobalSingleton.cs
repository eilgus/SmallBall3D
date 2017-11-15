using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFramework;


public class GlobalSingleton: XSingleton<GlobalSingleton>
{

    public string _name = "";
    public string _ip = "";

    public enum Mode
    {
        Alone,
        Network
    }

    public Mode mode;


    private GlobalSingleton()
    {
    }

    private void SetName(string name)
    {
        _name = name;
    }

    private void SetMode(string buttonName)
    {
        if (buttonName == "Alone")
            mode = Mode.Alone;
        else if (buttonName == "Network")
            mode = Mode.Network;
        else
            return;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XFramework;

public class SelectGUIControl : MonoBehaviour {

    protected string _name;
    GlobalSingleton globalSigton;

    // Use this for initialization
    void Start () {
        globalSigton = GlobalSingleton.GetInstance();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void OnConfirmButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnSelectRoom()
    {

    }

    public void OnButtonReturn()
    {
        SceneManager.LoadScene("1-StarScene");
    }

}

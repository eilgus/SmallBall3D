using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainManager : MonoBehaviour {

    [SerializeField]
    private GameObject settleImage;

    [SerializeField]
    private Text settleText;

    [SerializeField]
    private GameObject player;

    private Player host;


    // Use this for initialization
    void Start () {

        host = player.GetComponent<Player>();
    }
	
	// Update is called once per frame
	void Update () {
		if(host.hp <= 0)
        {
            settleImage.SetActive(true);
            settleText.text = "Game Over";
        }

        else if(host.enemyPlayer._hp<=0)
        {
            settleImage.SetActive(true);
            settleText.text = "You Win";
        }
	}
}

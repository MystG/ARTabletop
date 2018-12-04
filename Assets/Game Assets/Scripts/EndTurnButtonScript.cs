using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonScript : MonoBehaviour {

    public Button but;

    void Start()
    {
        but.onClick.AddListener(EndTurn);
    }

    void EndTurn()
    {
        PlayerManagerScript[] players = GameObject.FindObjectsOfType<PlayerManagerScript>();

        foreach (PlayerManagerScript pm in players)
        {
            if (pm.isLocalPlayer)
            {
                GameObject.Find("MoveUI").SetActive(false);
                GameObject.Find("Game Manager").GetComponent<GameManagerScript>().CmdNextTurn();
                return;
            }
        }
    }
}

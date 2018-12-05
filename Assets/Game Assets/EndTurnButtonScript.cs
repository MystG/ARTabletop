using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonScript : MonoBehaviour {

    public void EndTurn()
    {
        PlayerManager[] players = GameObject.FindObjectsOfType<PlayerManager>();

        foreach (PlayerManager pm in players)
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

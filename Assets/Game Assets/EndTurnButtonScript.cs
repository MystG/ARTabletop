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
                pm.EndTurn();
                return;
            }
        }
    }
}

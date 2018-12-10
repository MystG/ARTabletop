using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class EndTurnButtonScript : NetworkBehaviour {

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

    [ClientRpc]
    public void RpcSetEndTurnUI(bool val)
    {
        GameObject.Find("MoveUI").SetActive(val);
    }
}

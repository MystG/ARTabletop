using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StartGameButtonScript : NetworkBehaviour
{
    public void StartGame()
    {
        GameObject.FindObjectOfType<GameManagerScript>().CmdStartGame();

        //RpcRemoveStartUI();
    }

    [ClientRpc]
    public void RpcSetStartUI(bool val)
    {
        GameObject.Find("Start Game UI").SetActive(val);
    }

}

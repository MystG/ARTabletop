using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StartGameButtonScript : NetworkBehaviour
{
    public void StartGame()
    {
        GameObject.FindObjectOfType<GameManagerScript>().CmdStartGame();

        //UI: deactivate Start Game UI upon pressing
        GameObject.Find("Start Game UI").SetActive(false);
    }

    [ClientRpc]
    public void RpcSetStartUI(bool val)
    {
        GameObject.Find("Start Game UI").SetActive(val);
    }

}

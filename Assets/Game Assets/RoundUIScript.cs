using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RoundUIScript : NetworkBehaviour
{
    //private GameManagerScript gm;

    private Text PlayerText;
    private Text RoundText;
    private Text DayText;

    public override void OnStartClient()
    {
        PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        DayText = GameObject.Find("Day Text").GetComponent<Text>();
    }

    /*
    private void Update()
    {

    }
    */
    
    [ClientRpc]
    public void RpcUpdateTurnUI(int turn_player, int num_players, int round, int rounds_per_day, int day, int num_days)
    {
        PlayerText.text = "Player: " + (turn_player+1) + "/" + num_players;
        RoundText.text = "Round: " + round + "/" + rounds_per_day;
        DayText.text = "Day: " + day + "/" + num_days;
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        PlayerText.text = "Game Finished";
        RoundText.text = "";
        DayText.text = "";
    }
}

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
    //private Text VoteText;

    /*
    // Use this for initialization
    void Start () {

        PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        DayText = GameObject.Find("Day Text").GetComponent<Text>();
        VoteText = GameObject.Find("Vote Text").GetComponent<Text>();
    }
    */

    public override void OnStartClient()
    {
        PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        DayText = GameObject.Find("Day Text").GetComponent<Text>();
        //VoteText = GameObject.Find("Vote Text").GetComponent<Text>();
    }

    private void Update()
    {
        /*
        PlayerText.text = "Player: " + turn_player + "/" + num_players;
        RoundText.text = "Round: " + round + "/" + rounds_per_day;
        DayText.text = "Day: " + day + "/" + num_days;

        if (voting)
        {
            VoteText.text = "Votes are Being Cast";
        }
        else
        {
            VoteText.text = "";
        }
        */
    }
    
    [ClientRpc]
    public void RpcUpdateTurnUI(int turn_player, int num_players, int round, int rounds_per_day, int day, int num_days)
    {
        PlayerText.text = "Player: " + (turn_player+1) + "/" + num_players;
        RoundText.text = "Round: " + round + "/" + rounds_per_day;
        DayText.text = "Day: " + day + "/" + num_days;

        /*
        if (value == 0)
        {
            VoteText.text = "Votes are Being Cast";
        }
        else if (value == 1)
        {
            VoteText.text = "It's your move";
        }
        else
        {
            VoteText.text = "It's an opponent's move";
        }
        */
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        PlayerText.text = "Game Finished";
        RoundText.text = "";
        DayText.text = "";
    }
}

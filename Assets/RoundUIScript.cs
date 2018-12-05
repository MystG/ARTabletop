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
    private Text VoteText;

    // Use this for initialization
    void Start () {

        PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        DayText = GameObject.Find("Day Text").GetComponent<Text>();
        VoteText = GameObject.Find("Vote Text").GetComponent<Text>();
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
    void RpcUpdateTurnUI(int turn_player, int num_players, int round, int rounds_per_day, int day, int num_days, bool voting)
    {
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
    }
}

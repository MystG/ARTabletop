using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class GameManagerScript : NetworkBehaviour
{
    private int day = 1;
    private int round = 1;
    private int turn_player = 0;
    
    private int roundsPerDay = 2;
    private int numDays = 2;

    public GameObject machine;

    private SpaceManager[] spaces;
    private PlayerManagerScript[] players;
    private List<MachineScript> machines;

    private Dictionary<MachineScript, int> votes;
    private int num_votes = 0;

    // Use this for initialization
    void Start () {
        spaces = GameObject.FindObjectsOfType<SpaceManager>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    public void CmdStartGame()
    {
        if (!isServer)
            return;

        players = GameObject.FindObjectsOfType<PlayerManagerScript>();

        for(int i=0; i< players.Length; i++)
        {
            GameObject mach = Instantiate(machine, spaces[Random.Range(0, spaces.Length)].gameObject.transform);
            NetworkServer.Spawn(mach);
            machines.Add(mach.GetComponent<MachineScript>());
            mach.transform.SetParent(spaces[Random.Range(0, spaces.Length)].gameObject.transform);

            players[i].transform.SetParent(spaces[Random.Range(0, spaces.Length)].gameObject.transform);
        }

        turn_player = -1;
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {
        if (!isServer)
            return;

        turn_player++;

        if(turn_player >= players.Length)
        {
            turn_player = 0;
            round++;
        }

        if(round > roundsPerDay)
        {
            round = 1;
            day++;

            if (day > numDays)
            {
                //finish game
                print("end game");
            }
            else
            {
                turn_player = -1;

                //start voting turn
                foreach (PlayerManagerScript p in players)
                {
                    votes = new Dictionary<MachineScript, int>();
                    p.RpcStartVoteTurn();
                }
            }
        }
        else
        {
            foreach(MachineScript m in machines)
            {
                switch (round)
                {
                    case 0:
                        m.has_recources = true;
                        m.available_to_all = false;
                        break;
                    case 2:
                        m.available_to_all = true;
                        break;
                }
            }
            
            //change player's turn
            players[turn_player].RpcStartTurn();
        }
        
    }

    [Command]
    public void CmdCollectVote(MachineScript mach)
    {
        if (!isServer)
            return;

        if(votes.ContainsKey(mach))
        {
            votes[mach]++;
        }
        else
        {
            votes.Add(mach, 1);
        }

        num_votes++;

        if(num_votes >= players.Length)
        {
            int highest_num = 0;
            List<MachineScript> results = new List<MachineScript>();
            foreach(MachineScript m in votes.Keys)
            {
                if(votes[m] == highest_num)
                {
                    results.Add(m);
                }
                else if(votes[m] > highest_num)
                {
                    highest_num = votes[m];
                    results = new List<MachineScript>();
                    results.Add(m);
                }
            }

            MachineScript result = results[Random.Range(0, results.Count)];

            machines.Remove(result);
            NetworkServer.Destroy(result.gameObject);
        }
    }
}

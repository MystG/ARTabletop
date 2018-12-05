using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManagerScript : NetworkBehaviour
{

    [SyncVar] public int day = 1;
    [SyncVar] public int round = 1;
    [SyncVar] public int turn_player = 0;

    [SyncVar] public int roundsPerDay = 3;
    [SyncVar] public int numDays = 2;


    public GameObject machine;

    private List<SpaceManager> spaces;
    private List<PlayerManager> players;
    private List<Inventory> inventories;
    private List<MachineScript> machines;

    private Dictionary<MachineScript, int> votes;
    [SyncVar] private int num_votes = 0;
    [SyncVar] public bool voting = false;

    private Text PlayerText;
    private Text RoundText;
    private Text DayText;
    private Text VoteText;
    //private RoundUIScript roundUI;

    // Use this for initialization
    void Start()
    {
        spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        
        PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        DayText = GameObject.Find("Day Text").GetComponent<Text>();
        VoteText = GameObject.Find("Vote Text").GetComponent<Text>();
        

        //roundUI = FindObjectOfType<RoundUIScript>();
    }

    public void Update()
    {
        /*
        PlayerText.text = "Player: " + turn_player + "/" + players.Count;
        RoundText.text = "Round: " + round + "/" + roundsPerDay;
        DayText.text = "Day: " + day + "/" + numDays;

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

    [Command]
    public void CmdStartGame()
    {
        if (!isServer)
            return;

        //get list of all active players
        players = new List<PlayerManager>(GameObject.FindObjectsOfType<PlayerManager>());
        inventories = new List<Inventory>(GameObject.FindObjectsOfType<Inventory>());

        machines = new List<MachineScript>();

        //spawn the correct number of machines
        for (int i = 0; i < players.Count; i++)
        {
            GameObject mach = Instantiate(machine, spaces[Random.Range(0, spaces.Count)].gameObject.transform);
            NetworkServer.Spawn(mach);
            machines.Add(mach.GetComponent<MachineScript>());
        }

        //put the players and machines on random spaces
        int[] indexes = Permutation(spaces.Count);
        for (int i = 0; i < players.Count; i++)
        {
            //players[i].CmdSetLocation(spaces[indexes[i]].row, spaces[indexes[i]].col);
            players[i].row = spaces[indexes[i]].row;
            players[i].col = spaces[indexes[i]].col;
        }
        for (int i = 0; i < machines.Count; i++)
        {
            //machines[i].CmdSetLocation(spaces[indexes[i+ players.Count]].row, spaces[indexes[i + players.Count]].col);
            machines[i].row = spaces[indexes[i + players.Count]].row;
            machines[i].col = spaces[indexes[i + players.Count]].col;
        }

        //start the first turn
        turn_player = -1;
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {
        if (!isServer)
            return;

        //move to next player
        turn_player++;

        //if the last player in the round went,
        //start the next round with the first player
        if (turn_player >= players.Count)
        {
            turn_player = 0;
            round++;
        }

        //if it's tha last round of the day...
        if (round > roundsPerDay)
        {
            //start the rounds over and move to the next day
            round = 1;
            day++;

            foreach(Inventory inv in inventories)
            {
                inv.CmdAddValue(3, (int)Inventory.Indexes.HP);
                inv.CmdAddValue(10, (int)Inventory.Indexes.Energy);
            }

            if (day > numDays)
            {
                //finish game
                print("end game");
            }
            else
            {
                turn_player = -1;

                //ask each player to cast a vote
                foreach (PlayerManager p in players)
                {
                    votes = new Dictionary<MachineScript, int>();
                    foreach(MachineScript m in machines)
                    {
                        votes.Add(m, 0);
                    }
                    
                    voting = true;
                    p.RpcStartVoteTurn();
                }
            }
        }
        else
        {
            //depending on the round,
            //update the status of each machine's recources
            foreach (MachineScript m in machines)
            {
                switch (round)
                {
                    case 1:
                        m.has_recources = true;
                        m.available_to_all = false;
                        break;
                    case 3:
                        m.available_to_all = true;
                        break;
                }
            }

            //have the next player start their turn
            players[turn_player].RpcStartTurn();
        }

        //update ui
        PlayerText.text = "Player: " + turn_player + "/" + players.Count;
        RoundText.text = "Round: " + round + "/" + roundsPerDay;
        DayText.text = "Day: " + day + "/" + numDays;

        if (voting)
        {
            VoteText.text = "Votes are Being Cast";
        }
        else
        {
            VoteText.text = "";
        }
    }

    [Command]
    public void CmdCollectVote(int r, int c)
    {
        if (!isServer)
            return;

        Debug.Log("flag01");

        SpaceManager input = GetSpaceWithCoord(r, c);

        if (input)
        {
            Debug.Log("flag02");

            MachineScript mach = input.GetComponent<MachineScript>();

            if (mach && votes.ContainsKey(mach))
            {
                Debug.Log("flag03");
                votes[mach]++;
            }
        }
        
        num_votes++;

        if (num_votes >= players.Count)
        {
            Debug.Log("flag06");
            int highest_num = 0;
            List<MachineScript> results = new List<MachineScript>();

            foreach (MachineScript m in votes.Keys)
            {
                if (votes[m] == highest_num)
                {
                    results.Add(m);
                }
                else if (votes[m] > highest_num)
                {
                    highest_num = votes[m];
                    results = new List<MachineScript>();
                    results.Add(m);
                }
            }

            voting = true;

            MachineScript result = results[Random.Range(0, results.Count)];

            machines.Remove(result);
            NetworkServer.Destroy(result.gameObject);
            Debug.Log("flag07");
            CmdNextTurn();
        }
    }

    private int[] Permutation(int listSize)
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < listSize; i++) pool.Add(i);

        int[] result = new int[listSize];
        for (int i = 0; i < listSize; i++)
        {
            int index = Random.Range(0, pool.Count);
            int val = pool[index];
            result[i] = val;
            pool.RemoveAt(index);
        }

        return result;
    }

    private SpaceManager GetSpaceWithCoord(int r, int c)
    {
        foreach (SpaceManager s in spaces)
        {
            if (r == s.row && c == s.col)
            {
                return s;
            }
        }

        return null;
    }
}

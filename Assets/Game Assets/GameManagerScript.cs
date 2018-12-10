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

    [SyncVar] public int roundsPerDay;
    [SyncVar] public int numDays;

    public int HP_Regen;
    public int E_Regen;

    public GameObject machine_prefab;

    private List<SpaceManager> spaces;
    private List<PlayerManager> players;
    private List<Inventory> inventories;
    private List<MachineScript> machines;

    private Dictionary<Vector2, int> votes;
    [SyncVar] private int num_votes = 0;
    //[SyncVar] public bool is_voting_turn = false;
    public int machines_per_player;

    private RoundUIScript roundui;

    private StartGameButtonScript startui;

    private Vector3[] player_colors = new Vector3[]
    {
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(1,1,0),
        new Vector3(1,0,1),
        new Vector3(0,1,1),
        new Vector3(1,1,1),
        new Vector3(0,0,0)
    };

    //private Text PlayerText;
    //private Text RoundText;
    //private Text DayText;
    //private Text VoteText;
    //private RoundUIScript roundUI;

    /*
    // Use this for initialization
    void Start()
    {
        spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        roundui = GameObject.FindObjectOfType<RoundUIScript>();

        startui = GameObject.FindObjectOfType<StartGameButtonScript>();

        //PlayerText = GameObject.Find("Player Text").GetComponent<Text>();
        //RoundText = GameObject.Find("Round Text").GetComponent<Text>();
        //DayText = GameObject.Find("Day Text").GetComponent<Text>();
        //VoteText = GameObject.Find("Vote Text").GetComponent<Text>();
    }
    */

    public override void OnStartClient()
    {
        spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        roundui = GameObject.FindObjectOfType<RoundUIScript>();

        startui = GameObject.FindObjectOfType<StartGameButtonScript>();
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

        //update ui
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

        day = 1;
        round = 1;
        turn_player = 0;

        //remove the start ui from all clients
        startui.RpcSetStartUI(false);

        //get list of all active players
        players = new List<PlayerManager>(GameObject.FindObjectsOfType<PlayerManager>());
        /*
        for(int i=0; i<players.Count; i++)
        {
            players[i].id = i;
        }
        */

        //get list of active inventories
        inventories = new List<Inventory>(GameObject.FindObjectsOfType<Inventory>());

        machines = new List<MachineScript>();

        //spawn the correct number of machines
        for (int i = 0; i < players.Count; i++)
        {
            //spawn machines
            for (int x = 0; x < machines_per_player; x++)
            {
                GameObject mach = Instantiate(machine_prefab);
                NetworkServer.Spawn(mach);
                machines.Add(mach.GetComponent<MachineScript>());
            }
            
            //set each player's color to something unique
            players[i].RpcSetColor(player_colors[i].x, player_colors[i].y, player_colors[i].z);
        }

        GameObject.FindObjectOfType<LocalPlayerIndicator>().RpcActivate();

        //put the players and machines on random spaces
        int[] indexes = IndexPermutation(spaces.Count);
        int j = 0;
        foreach(PlayerManager p in players)
        {
            p.gameObject.GetComponent<LocationManager>().CmdSetLocation(spaces[indexes[j]].row, spaces[indexes[j]].col);
            j++;
        }
        foreach (MachineScript m in machines)
        {
            m.gameObject.GetComponent<LocationManager>().CmdSetLocation(spaces[indexes[j]].row, spaces[indexes[j]].col);
            j++;
        }
        /*
        for (int i = 0; i < players.Count; i++)
        {
            //players[i].CmdSetLocation(spaces[indexes[i]].row, spaces[indexes[i]].col);
            //players[i].row = spaces[indexes[i]].row;
            //players[i].col = spaces[indexes[i]].col;
            players[i].gameObject.GetComponent<LocationManager>().CmdSetLocation(spaces[indexes[i]].row, spaces[indexes[i]].col);
        }
        for (int i = 0; i < machines.Count; i++)
        {
            //machines[i].CmdSetLocation(spaces[indexes[i+ players.Count]].row, spaces[indexes[i + players.Count]].col);
            //machines[i].row = spaces[indexes[i + players.Count]].row;
            //machines[i].col = spaces[indexes[i + players.Count]].col;
            players[i].gameObject.GetComponent<LocationManager>().CmdSetLocation(spaces[indexes[i]].row, spaces[indexes[i]].col);
        }
        */

        //start the first turn
        turn_player = -1;
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {
        if (!isServer)
            return;

        if(turn_player >= 0 && turn_player < players.Count)
        {
            GetComponent<NetworkIdentity>().RemoveClientAuthority(players[turn_player].connectionToClient);
        }

        //move to next player
        turn_player++;

        //if the last player in the round went,
        //start the next round with the first player
        if (turn_player >= players.Count)
        {
            turn_player = 0;
            round++;
        }

        if(round > roundsPerDay)
        {
            round = 0;
            day++;

            foreach (Inventory inv in inventories)
            {
                inv.CmdAddValue(HP_Regen, (int)Inventory.Indexes.HP);
                inv.CmdAddValue(E_Regen, (int)Inventory.Indexes.Energy);
            }
        }

        if (day > numDays)
        {
            //finish game
            roundui.RpcEndGame();
            return;
        }

        NetworkConnection connect = players[turn_player].connectionToClient;
        GetComponent<NetworkIdentity>().AssignClientAuthority(connect);
        if (round==0)
        {
            //if it's the first vote bing asked for
            //create a new list to keep track of votes
            if (turn_player==0)
            {
                votes = new Dictionary<Vector2, int>();
                foreach (MachineScript m in machines)
                {
                    LocationManager lm = m.gameObject.GetComponent<LocationManager>();
                    votes.Add(new Vector2(lm.row, lm.col), 0);
                }
            }

            players[turn_player].TargetStartVoteTurn(connect);
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
                    case 4:
                        m.available_to_all = true;
                        break;
                }
            }

            //have the next player start their turn
            //a dn give client authority to that player
            players[turn_player].TargetStartMoveTurn(connect);
        }
        
        roundui.RpcUpdateTurnUI(turn_player, players.Count, round, roundsPerDay, day, numDays);

        /*
        //if it's tha last round of the day...
        if (round > roundsPerDay)
        {
            //start the rounds over and move to the next day
            round = 0;
            day++;

            foreach(Inventory inv in inventories)
            {
                inv.CmdAddValue(HP_Regen, (int)Inventory.Indexes.HP);
                inv.CmdAddValue(E_Regen, (int)Inventory.Indexes.Energy);
            }

            if (day > numDays)
            {
                //finish game
                print("end game");
            }
            else
            {
                //turn_player = -1;

                //create a new list to keep track of votes
                votes = new Dictionary<MachineScript, int>();
                foreach (MachineScript m in machines)
                {
                    votes.Add(m, 0);
                }

                is_voting_turn = true;

                //ask each player to cast a vote
                NetworkConnection connect = players[turn_player].connectionToClient;
                GetComponent<NetworkIdentity>().AssignClientAuthority(connect);
                players[turn_player].TargetStartVoteTurn(connect);
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
                    case 4:
                        m.available_to_all = true;
                        break;
                }
            }

            //have the next player start their turn
            //a dn give client authority to that player
            NetworkConnection connect = players[turn_player].connectionToClient;
            GetComponent<NetworkIdentity>().AssignClientAuthority(connect);
            players[turn_player].TargetStartMoveTurn(connect);
        }
        */
    }

    [Command]
    public void CmdCollectVote(int r, int c)
    {
        if (!isServer)
            return;

        //SpaceManager input = GetSpaceWithCoord(r, c);

        Vector2 loc = new Vector2(r, c);

        if (votes.ContainsKey(loc))
        {
            votes[loc]++;
        }

        num_votes++;

        //if all the votes were cast
        if (num_votes >= players.Count)
        {
            //get the machine(s) with the highest number of votes
            int highest_num = 0;
            List<Vector2> results = new List<Vector2>();

            foreach (Vector2 m in votes.Keys)
            {
                if (votes[m] == highest_num)
                {
                    results.Add(m);
                }
                else if (votes[m] > highest_num)
                {
                    highest_num = votes[m];
                    results.Clear();
                    results.Add(m);
                }
            }

            //remove the machine with the most votes (random if there is a tie)
            Vector2 result = results[Random.Range(0, results.Count)];
            MachineScript mach = GetSpaceWithCoord((int)result.x, (int)result.y).gameObject.GetComponentInChildren<MachineScript>();
            machines.Remove(mach);
            NetworkServer.Destroy(mach.gameObject);
            
            //is_voting_turn = false;
        }

        //start the next turn
        CmdNextTurn();
    }

    private int[] IndexPermutation(int listSize)
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

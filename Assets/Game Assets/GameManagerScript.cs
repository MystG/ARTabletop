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
    public int machines_per_player;

    private RoundUIScript roundui;

    //private StartGameButtonScript startui;

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

    public override void OnStartClient()
    {
        spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        roundui = GameObject.FindObjectOfType<RoundUIScript>();

        //startui = GameObject.FindObjectOfType<StartGameButtonScript>();
    }

    /*
    public void Update()
    {
        
    }
    */    

    [Command]
    public void CmdStartGame()
    {
        if (!isServer)
            return;

        day = 1;
        round = 1;
        turn_player = 0;

        //UI: remove the start ui from all clients
        //startui.RpcSetStartUI(false);

        //get list of all active players
        players = new List<PlayerManager>(GameObject.FindObjectsOfType<PlayerManager>());

        //get list of active inventories
        inventories = new List<Inventory>(GameObject.FindObjectsOfType<Inventory>());

        machines = new List<MachineScript>();
        
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

            //indicate the game started to each player
            players[i].RpcStartGame();
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

        //start the first turn
        turn_player = -1;
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {
        if (!isServer)
            return;

        //remove authority of this object from last player
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

        //assign authority of this object to the turn player
        NetworkConnection connect = players[turn_player].connectionToClient;
        GetComponent<NetworkIdentity>().AssignClientAuthority(connect);

        //after the last round that day
        //set it to the voting round (0)
        //and regenerate players HP and Energy
        if (round > roundsPerDay)
        {
            round = 0;
            day++;

            foreach (Inventory inv in inventories)
            {
                inv.CmdSetValue(HP_Regen, (int)Inventory.Indexes.HP);
                inv.CmdAddValue(E_Regen, (int)Inventory.Indexes.Energy);
            }
        }

        //after the last day, the game is over
        if (day > numDays)
        {
            //finish game
            roundui.RpcEndGame();
            return;
        }
        
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
            players[turn_player].TargetStartMoveTurn(connect);
        }
        
        //UI: update the ui the display the turn_player, the round, and the day
        roundui.RpcUpdateTurnUI(turn_player, players.Count, round, roundsPerDay, day, numDays);
    }

    [Command]
    public void CmdCollectVote(int r, int c)
    {
        if (!isServer)
            return;

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

    
    [Command]
    public void CmdAttack(int atk_r, int atk_c, int def_r, int def_c, int damage, float coin_percent, int max_knockback)
    {
        SpaceManager atk_space = GetSpaceWithCoord(atk_r, atk_c);
        SpaceManager def_space = GetSpaceWithCoord(def_r, def_c);

        PlayerManager attacker = atk_space.GetComponentInChildren<PlayerManager>();
        PlayerManager defender = def_space.GetComponentInChildren<PlayerManager>();

        Inventory atk_inv = atk_space.GetComponentInChildren<Inventory>();
        Inventory def_inv = def_space.GetComponentInChildren<Inventory>();

        if (!attacker || !defender || !atk_inv || !def_inv)
        {
            return;
        }

        //deal damage to them
        def_inv.CmdAddValue(-damage, (int)Inventory.Indexes.HP);

        //take a percentage of their coins
        int coin_take = (int) (coin_percent * def_inv.GetValue((int)Inventory.Indexes.Coins));
        def_inv.CmdAddValue(-coin_take, (int)Inventory.Indexes.Coins);
        atk_inv.CmdAddValue(coin_take, (int)Inventory.Indexes.Coins);

        //knock player back to a space max_knockback_dist away and is further from the player
        //if there is no space, decrese max by one and try again
        List<SpaceManager> knockback_spaces = new List<SpaceManager>();
        int max = max_knockback;
        while (knockback_spaces.Count == 0 && max > 0)
        {
            foreach (SpaceManager s in spaces)
            {
                //if the space is as far away as possible,
                //and the new space is farther from the attacker than from the defender
                //and the new space will place the defender farther from the attacker than they were
                if (s.DistanceTo(def_space) == max
                    && s.DistanceTo(atk_space) > s.DistanceTo(def_space)
                    &&  s.DistanceTo(atk_space) > def_space.DistanceTo(atk_space))
                {
                    knockback_spaces.Add(s);
                }
            }

            if(knockback_spaces.Count >= 1)
            {
                SpaceManager k = knockback_spaces[Random.Range(0, knockback_spaces.Count)];
                defender.gameObject.GetComponent<LocationManager>().CmdSetLocation(k.row, k.col);
                break;
            }

            max -= 1;
        }
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

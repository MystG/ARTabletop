using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    //private List<SpaceManager> spaces;

    private GameManagerScript gm;
    //private MachineScript[] machines;
    private SpaceManager[] spaces;
    private MoveButtonScript[] move_buttons;

    private GameObject MoveUI;

    private Inventory inventory;
    private LocationManager location_manager;

    public bool can_move = false;
    public bool can_vote = false;

    public int atk_damage;
    public float coin_take_percent;
    public int min_knockback_dist;
    public int max_knockback_dist;

    private Text TurnText;

    //[SyncVar] public int id = -1;

    //[SyncVar] public int row;
    //[SyncVar] public int col;
    
    /*
    // Use this for initialization
    void Start()
    {
        gm = GameObject.FindObjectOfType<GameManagerScript>();
        //machines = GameObject.FindObjectsOfType<MachineScript>();
        
        inventory = this.gameObject.GetComponent<Inventory>();
        location_manager = this.gameObject.GetComponent<LocationManager>();

        //spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        move_buttons = GameObject.FindObjectsOfType<MoveButtonScript>();

        TurnText = GameObject.Find("Turn Text").GetComponent<Text>();

        //machines = GameObject.FindObjectsOfType<MachineScript>();
    }
    */

    public override void OnStartClient()
    {
        gm = GameObject.FindObjectOfType<GameManagerScript>();

        inventory = this.gameObject.GetComponent<Inventory>();
        location_manager = this.gameObject.GetComponent<LocationManager>();

        spaces = GameObject.FindObjectsOfType<SpaceManager>();
        move_buttons = GameObject.FindObjectsOfType<MoveButtonScript>();

        TurnText = GameObject.Find("Turn Text").GetComponent<Text>();

        MoveUI = GameObject.Find("MoveUI");

        //MoveUI.SetActive(false);
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    void Update()
    {
        /*
        if(transform.parent == transform.root)
        {
            transform.SetParent(spaces[0].gameObject.transform);
        }
        */

        /*
        SpaceManager current_space = GetComponentInParent<SpaceManager>();

        if (!(current_space && row == current_space.row && col == current_space.col))
        {
            //find the space corresponding to the given coordinates
            SpaceManager s = GetSpaceWithCoord(row, col);
            if (s) transform.SetParent(s.gameObject.transform);

            transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        */
        
        //if it's the voting phase and a machine was clicked, cast the vote
        if (isLocalPlayer && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool collided = Physics.Raycast(ray, out hit);
            
            if (can_vote && collided && hit.collider.gameObject.GetComponent<MachineScript>() != null)
            {
                can_vote = false;

                LocationManager mach_loc = hit.collider.gameObject.GetComponent<LocationManager>();

                hit.collider.gameObject.GetComponent<Collider>().enabled = false;

                TurnText.text = "Waiting for votes to be cast";

                //gm.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
                gm.CmdCollectVote(mach_loc.row, mach_loc.col);
                //gm.GetComponent<NetworkIdentity>().RemoveClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
            }
            else if(can_move && collided && hit.collider.gameObject.GetComponent<PlayerManager>() != null)
            {
                List<PlayerManager> players = new List<PlayerManager>(GameObject.FindObjectsOfType<PlayerManager>());
                PlayerManager hit_player = hit.collider.gameObject.GetComponent<PlayerManager>();

                if (hit_player != this && players.Contains(hit_player))
                {
                    AttackPlayer(hit_player);
                }
            }
        }
    }

    /*
    [Command]
    public void CmdSetLocation(int r, int c)
    {
        foreach (SpaceManager s in spaces)
        {
            if (r == s.row && c == s.col)
            {
                row = r;
                col = c;
                
                return;
            }
        }
    }
    */

    //moves this player on the server (which is then synced accross clients
    [Command]
    public void CmdMove(int r, int c)
    {
        //SpaceManager destination = GetSpaceWithCoord(r, c);
        SpaceManager destination = location_manager.Get_Space(r, c);
        SpaceManager current_space = transform.parent.GetComponent<SpaceManager>();

        int E = inventory.GetValue((int)Inventory.Indexes.Energy);

        //if the move can be made...
        if (current_space && current_space.ValidMove(destination, E))
        {
            int dist = current_space.DistanceTo(destination);

            //make the move
            //CmdSetLocation(destination.row, destination.col);
            //row = r;
            //col = c;
            location_manager.CmdSetLocation(r, c);

            //grant the proper recources to the player
            MachineScript mach = destination.GetComponentInChildren<MachineScript>();

            //if the machine doesn't have an owner, set it to the player
            bool aquired = false;
            if(mach && mach.owner == null)
            {
                mach.RpcSetOwner();
                Color color = GetComponent<MeshRenderer>().material.color;
                mach.RpcSetColor(color.r, color.g, color.b);
                aquired = true;
            }

            //if the machine has recources and the player can get them,
            //give them to the player
            if (mach && mach.has_recources && (this == mach.owner || mach.available_to_all || aquired))
            {
                inventory.CmdAddValue(mach.recource_amount, mach.recource_type);
                mach.has_recources = false;
            }

            //subtract Energy from the player
            inventory.CmdAddValue(-dist, (int)Inventory.Indexes.Energy);
        }
    }

    //start the move turn of this player by enableing the Moveturn ui on the client,
    //activating the buttons of legal moves
    [TargetRpc]
    public void TargetStartMoveTurn(NetworkConnection target)
    {
        //if (!isLocalPlayer)
        //{
          //  return;
        //}

        //gm.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);

        MoveUI.SetActive(true);
        can_move = true;

        //set all buttons for spaces which have valid moves to active, and inactive otherwise
        foreach (MoveButtonScript but in move_buttons)
        {
            SpaceManager destination = but.GetComponentInParent<SpaceManager>();
            SpaceManager current_space = location_manager.CurrentSpace();

            int E = inventory.GetValue((int)Inventory.Indexes.Energy);

            but.gameObject.SetActive(current_space && current_space.ValidMove(destination, E));
        }

        TurnText.text = "Move or End Your Turn";
    }

    //start the move turn of this player by enableing can_vote on each client
    [TargetRpc]
    public void TargetStartVoteTurn(NetworkConnection target)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        can_vote = true;

        MachineScript[] machines = GameObject.FindObjectsOfType<MachineScript>();
        foreach (MachineScript m in machines)
        {
            m.gameObject.GetComponent<Collider>().enabled = true;
        }

        TurnText.text = "Cast your vote";
    }

    public void AttackPlayer(PlayerManager other)
    {
        Inventory other_inv = other.gameObject.GetComponent<Inventory>();

        //deal damage to them
        other_inv.CmdAddValue(-atk_damage, (int)Inventory.Indexes.HP);

        //take a percentage of their coins
        int coin_take = (int) (coin_take_percent / other_inv.GetValue((int)Inventory.Indexes.HP));
        other_inv.CmdAddValue(-coin_take, (int)Inventory.Indexes.Coins);
        inventory.CmdAddValue(coin_take, (int)Inventory.Indexes.Coins);

        SpaceManager myloc = gameObject.GetComponent<LocationManager>().CurrentSpace();
        SpaceManager otherlco = other.gameObject.GetComponent<LocationManager>().CurrentSpace();

        List<SpaceManager> knockback_spaces = new List<SpaceManager>();
        int max = max_knockback_dist;
        while (knockback_spaces.Count == 0 && max > 0)
        {
            foreach (SpaceManager s in spaces)
            {
                if (s.DistanceTo(otherlco) == max
                    && s.DistanceTo(myloc) > otherlco.DistanceTo(myloc))
                {
                    knockback_spaces.Add(s);
                }
            }

            if(knockback_spaces.Count >= 1)
            {
                SpaceManager k = knockback_spaces[Random.Range(0, knockback_spaces.Count)];
                other.gameObject.GetComponent<LocationManager>().CmdSetLocation(k.row, k.col);
                break;
            }

            max -= 1;
        }
    }

    public void EndTurn()
    {
        TurnText.text = "Opponent's Turn";

        MoveUI.SetActive(false);

        gm.CmdNextTurn();
    }

    [ClientRpc]
    public void RpcSetColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    /*
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
    */
}

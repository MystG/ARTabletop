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
    private MoveButtonScript[] move_buttons;

    private GameObject MoveUI;

    private Inventory inventory;
    private LocationManager location_manager;

    public bool can_move = false;
    public bool can_vote = false;

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
        if (isLocalPlayer && can_vote && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool collided = Physics.Raycast(ray, out hit);
            
            if (collided && hit.collider.gameObject.GetComponent<MachineScript>() != null)
            {
                can_vote = false;

                LocationManager mach_loc = hit.collider.gameObject.GetComponent<LocationManager>();

                hit.collider.gameObject.GetComponent<Collider>().enabled = false;

                TurnText.text = "Waiting for votes to be cast";

                //gm.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
                gm.CmdCollectVote(mach_loc.row, mach_loc.col);
                //gm.GetComponent<NetworkIdentity>().RemoveClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
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
                mach.RpcSetColor(color.r, color.b, color.g);
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

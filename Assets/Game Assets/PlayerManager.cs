using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour
{

    private List<SpaceManager> spaces;

    private GameManagerScript gm;
    private MachineScript[] machines;
    private MoveButtonScript[] move_buttons;

    private GameObject MoveUI;

    private Inventory inventory;

    public bool can_move = false;
    public bool can_vote = false;

    [SyncVar] public int row;
    [SyncVar] public int col;

    // Use this for initialization
    void Start()
    {
        gm = GameObject.FindObjectOfType<GameManagerScript>();
        //machines = GameObject.FindObjectsOfType<MachineScript>();
        MoveUI = GameObject.Find("MoveUI");
        MoveUI.SetActive(false);

        inventory = this.gameObject.GetComponent<Inventory>();

        spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());

        move_buttons = GameObject.FindObjectsOfType<MoveButtonScript>();
        foreach(MoveButtonScript but in move_buttons)
        {
            but.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        /*
        if(transform.parent == transform.root)
        {
            transform.SetParent(spaces[0].gameObject.transform);
        }
        */
        
        SpaceManager current_space = GetComponentInParent<SpaceManager>();

        if (current_space && row == current_space.row && col == current_space.col)
        {
            return;
        }

        //find the space corresponding to the given coordinates
        SpaceManager s = GetSpaceWithCoord(row, col);
        if(s) transform.SetParent(s.gameObject.transform);

        transform.localPosition = Vector3.zero;

        if (!isLocalPlayer)
            return;

        //if it's the voting phase and a machine was clicked, cast the vote
        if (can_vote && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool collided = Physics.Raycast(ray, out hit);

            if (collided && hit.collider.gameObject.GetComponent<MachineScript>() != null)
            {
                can_vote = false;

                MachineScript v = hit.collider.gameObject.GetComponent<MachineScript>();
                gm.CmdCollectVote(v.row, v.col);
            }
        }
    }

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

    //moves this player on the server (which is then synced accross clients
    [Command]
    public void CmdMove(int r, int c)
    {
        SpaceManager destination = GetSpaceWithCoord(r, c);
        SpaceManager current_space = transform.parent.GetComponent<SpaceManager>();

        int E = inventory.GetValue((int)Inventory.Indexes.Energy);

        //if the move can be made...
        if (current_space && current_space.ValidMove(destination, E))
        {
            int dist = current_space.DistanceTo(destination);

            //make the move
            //CmdSetLocation(destination.row, destination.col);
            row = r;
            col = c;

            //grant the proper recources to the player
            MachineScript mach = destination.GetComponentInChildren<MachineScript>();
            if(mach) GiveRecources(destination.GetComponentInChildren<MachineScript>());

            //subtract Energy from the player
            inventory.CmdAddValue(dist, (int)Inventory.Indexes.Energy);
        }
    }

    //start the move turn of this player by enableing the Moveturn ui on the client,
    //activating the buttons of legal moves
    [ClientRpc]
    public void RpcStartTurn()
    {
        if (isLocalPlayer)
        {
            MoveUI.SetActive(true);
            can_move = true;

            foreach (MoveButtonScript but in move_buttons)
            {
                SpaceManager destination = but.GetComponentInParent<SpaceManager>();
                SpaceManager current_space = transform.parent.GetComponent<SpaceManager>();
                int E = inventory.GetValue((int)Inventory.Indexes.Energy);

                but.gameObject.SetActive(current_space && current_space.ValidMove(destination, E));
            }
        }
    }

    //start the move turn of this player by enableing can_vote on the client
    [ClientRpc]
    public void RpcStartVoteTurn()
    {
        if (isLocalPlayer)
        {
            can_vote = true;
        }
    }
    
    public void GiveRecources(MachineScript mach)
    {
        if (!isServer)
        {
            return;
        }

        if (!mach.has_recources)
        {
            return;
        }

        if (this == mach.owner || mach.available_to_all)
        {
            inventory.CmdAddValue(3, (int)Inventory.Indexes.Grain);
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

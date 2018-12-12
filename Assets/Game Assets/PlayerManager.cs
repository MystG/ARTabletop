using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    private GameManagerScript gm;
    private MoveButtonScript[] move_buttons;
    private List<PlayerManager> players = new List<PlayerManager>();

    private GameObject MoveUI;

    private Inventory inventory;
    private LocationManager location_manager;

    public bool can_move = false;
    public bool can_vote = false;

    public int atk_damage;
    public float coin_take_percent;
    public int max_knockback_dist;
    public int attack_range;

    private Text TurnText;

    private Transform crownloc;
    public GameObject crownPrefab;

    public override void OnStartClient()
    {
        gm = GameObject.FindObjectOfType<GameManagerScript>();

        inventory = this.gameObject.GetComponent<Inventory>();
        location_manager = this.gameObject.GetComponent<LocationManager>();

        move_buttons = GameObject.FindObjectsOfType<MoveButtonScript>();

        //UI: get a reference to the text of the object that gives instructions
        TurnText = GameObject.Find("Turn Text").GetComponent<Text>();

        //UI: get reference to te object which has the "end turn" button
        MoveUI = GameObject.Find("MoveUI");

        GetComponent<Collider>().enabled = false;

        crownloc = transform.Find("crownloc");
    }

    public override void OnStartLocalPlayer()
    {
        //GetComponent<MeshRenderer>().material.color = Color.blue;

        if (!isServer)
        {
            //UI: disbles (hides) the UI element whic starts the game for non-serer objects
            //this is becuase non-server players are unable to use it (possible bug)
            GameObject.Find("Start Game UI").SetActive(false);
        }
    }

    void Update()
    {
        Vector3 pos;
        bool input_down = InputPos(out pos);

        if (input_down && isLocalPlayer)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit;
            bool collided = Physics.Raycast(ray, out hit);

            //if it's the voting phase and a machine was clicked, cast the vote
            if (can_vote && collided && hit.collider.gameObject.GetComponent<MachineScript>() != null)
            {
                can_vote = false;

                LocationManager mach_loc = hit.collider.gameObject.GetComponent<LocationManager>();

                //disable colliders on all machines
                MachineScript[] machines = GameObject.FindObjectsOfType<MachineScript>();
                foreach (MachineScript m in machines)
                {
                    m.gameObject.GetComponent<Collider>().enabled = false;
                }

                //UI: after a vote is cast, the ui states that other votes are being cast now
                TurnText.text = "Waiting for votes to be cast";

                gm.CmdCollectVote(mach_loc.row, mach_loc.col);
            }
            else if (can_move && collided && hit.collider.gameObject.GetComponent<PlayerManager>() != null)
            {
                //if you can attack and click an enemy, attack it
                PlayerManager hit_player = hit.collider.gameObject.GetComponent<PlayerManager>();

                SpaceManager otherLoc = hit_player.GetComponent<LocationManager>().CurrentSpace();
                SpaceManager myLoc = location_manager.CurrentSpace();

                if (hit_player != this && players.Contains(hit_player) && myLoc.DistanceTo(otherLoc) <= attack_range)
                {
                    can_move = false;

                    foreach (PlayerManager p in players)
                    {
                        p.GetComponent<Collider>().enabled = false;

                        p.gameObject.GetComponentInChildren<PlayerBillboard>().can_be_attacked = false;
                    }

                    //UI: after attacking, there is nothing left to do, so tell player to end their turn
                    TurnText.text = "End your turn";

                    gm.CmdAttack(myLoc.row, myLoc.col, otherLoc.row, otherLoc.col, atk_damage, coin_take_percent, max_knockback_dist);
                }
            }
        }
    }

    [ClientRpc]
    public void RpcStartGame()
    {
        players = new List<PlayerManager>(GameObject.FindObjectsOfType<PlayerManager>());

        foreach (MoveButtonScript m in move_buttons)
        {
            m.gameObject.SetActive(false);
        }

        MoveUI.SetActive(false);
    }

    //moves this player on the server (which is then synced accross clients)
    [Command]
    public void CmdMove(int r, int c)
    {
        SpaceManager destination = location_manager.Get_Space(r, c);
        SpaceManager current_space = transform.parent.GetComponent<SpaceManager>();

        int E = inventory.GetValue((int)Inventory.Indexes.Energy);
        int HP = inventory.GetValue((int)Inventory.Indexes.HP);

        //if the move can be made...
        if (current_space && current_space.ValidMove(destination, E) && HP > 0)
        {
            int dist = current_space.DistanceTo(destination);

            //set the player's location to the new space
            location_manager.CmdSetLocation(r, c);

            MachineScript mach = destination.GetComponentInChildren<MachineScript>();

            //if the machine doesn't have an owner, set it to the player
            bool aquired = false;
            if (mach && mach.owner == null)
            {
                Color color = GetComponent<MeshRenderer>().material.color;
                aquired = true;
                mach.RpcSetColor(color.r, color.g, color.b);
                mach.RpcSetOwner();
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
        MoveUI.SetActive(true);
        can_move = inventory.GetValue((int)Inventory.Indexes.HP) > 0;

        //if the player can't move, skip their turn
        if (!can_move)
        {
            TurnText.text = "You have no HP.\n End your turn/";
            return;
        }

        //set all buttons for spaces which have valid moves to active, and inactive otherwise
        foreach (MoveButtonScript but in move_buttons)
        {
            SpaceManager destination = but.GetComponentInParent<SpaceManager>();
            SpaceManager current_space = location_manager.CurrentSpace();

            int E = inventory.GetValue((int)Inventory.Indexes.Energy);

            but.gameObject.SetActive(current_space && current_space.ValidMove(destination, E));
        }

        foreach (PlayerManager p in players)
        {
            //if the other player can be attacked, say so on the UI
            SpaceManager otherLoc = p.gameObject.GetComponent<LocationManager>().CurrentSpace();
            SpaceManager myLoc = location_manager.CurrentSpace();

            if (myLoc.DistanceTo(otherLoc) <= attack_range & p != this)
            {
                p.gameObject.GetComponentInChildren <PlayerBillboard>().can_be_attacked = true;

                //enable the collider so it can be tapped
                p.GetComponent<Collider>().enabled = true;
            }
            else
            {
                p.gameObject.GetComponentInChildren<PlayerBillboard>().can_be_attacked = false;
            }
        }

        //UI: when your turn starts, give instructions of possible actions
        TurnText.text = "Move by pressing a button\nAttack by clicking a player\nor End Your Turn";
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

        //enable the colliders on all the machines
        MachineScript[] machines = GameObject.FindObjectsOfType<MachineScript>();
        foreach (MachineScript m in machines)
        {
            m.gameObject.GetComponent<Collider>().enabled = true;
        }

        //UI: when it is your turn to vote, say so
        TurnText.text = "Cast your vote";
    }

    public void EndTurn()
    {
        //UI: after you end your turn, say it is your opponent's turn
        TurnText.text = "Opponent's Turn";

        //UI: hide the ui that contains the End Turn button
        MoveUI.SetActive(false);

        gm.CmdNextTurn();
    }

    public void ActivateMove(int r, int c)
    {
        can_move = false;

        foreach (MoveButtonScript b in move_buttons)
        {
            b.gameObject.SetActive(false);
        }

        //UI: after moving, there is nothing left to do, so tell player to end their turn
        TurnText.text = "End your turn";

        CmdMove(r, c);
    }

    [ClientRpc]
    public void RpcSetColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    [ClientRpc]
    public void RpcIdentifyWinnder(bool is_winner)
    {
        if(is_winner)
        {
            GameObject.Instantiate(crownPrefab, crownloc);
        }
    }

    public bool InputPos(out Vector3 result)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                result = touch.position;
                return true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            result = Input.mousePosition;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}

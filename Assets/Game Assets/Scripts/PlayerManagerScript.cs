using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManagerScript : NetworkBehaviour
{
    private GameManagerScript gm;
    private MachineScript[] machines;

    private GameObject MoveUI;

    private Inventory inventory;

    public bool can_move = false;
    public bool can_vote = false;

	// Use this for initialization
	void Start () {
        gm = GameObject.FindObjectOfType<GameManagerScript>();
        machines = GameObject.FindObjectsOfType<MachineScript>();
        MoveUI = GameObject.Find("MoveUI");

        inventory = this.gameObject.GetComponent<Inventory>();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (can_vote && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool collided = Physics.Raycast(ray, out hit);

            if(collided && hit.collider.gameObject.tag.Equals("Machine"))
            {
                can_vote = false;
                gm.CmdCollectVote(hit.collider.gameObject.GetComponent<MachineScript>());
            }
        }
    }

    //moves this player on the server (which is then synced accross clients
    [Command]
    public void CmdMove(SpaceManager destination)
    {
        SpaceManager current_space = transform.parent.GetComponent<SpaceManager>();
        int E = inventory.GetValue((int)Inventory.Indexes.Energy);

        if (current_space && E>0 && current_space.ValidMove(destination))
        {
            transform.parent = destination.gameObject.transform;
            transform.position = new Vector3(0, 10, 0);

            inventory.AddValue(-3, (int)Inventory.Indexes.Energy);

            if (destination.GivesPlayerRecources(this))
            {
                inventory.AddValue(3, (int)Inventory.Indexes.Grain);

                destination.GetComponentInChildren<MachineScript>().has_recources = false;
            }
        }
    }

    //start the move turn of this player by enableing the Moveturn ui on the client,
    //activating the buttons of legal moves
    [ClientRpc]
    public void RpcStartTurn()
    {
        if(isLocalPlayer)
        {
            MoveUI.SetActive(true);
            can_move = true;
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
}

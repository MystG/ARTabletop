﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MachineScript : NetworkBehaviour
{
    public PlayerManager owner = null;

    [SyncVar] public bool available_to_all = false;

    [SyncVar] public bool has_recources = false;

    public int recource_amount = 3;
    public int recource_type = (int)Inventory.Indexes.Coins;

    //private Collider col;

        /*
    // Use this for initialization
    void Start()
    {
        //spaces = new List<SpaceManager>(GameObject.FindObjectsOfType<SpaceManager>());
        //col = GetComponent<Collider>();
        GetComponent<Collider>().enabled = false;
    }
    */

    public override void OnStartClient()
    {
        GetComponent<Collider>().enabled = false;
    }

    [ClientRpc]
    public void RpcSetOwner()
    {
        GameObject space = transform.parent.gameObject;
        PlayerManager sibling = space.GetComponentInChildren<PlayerManager>();
        if (sibling)
        {
            owner = sibling;
        }
    }

    [ClientRpc]
    public void RpcSetColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    /*
    [ClientRpc]
    public void SetCollider(bool val)
    {
        col.enabled = val;
    }
    */

    //[SyncVar] public int row;
    //[SyncVar] public int col;

    //private List<SpaceManager> spaces;

    /*


/*
// Update is called once per frame
void Update()
{
    SpaceManager current_space = GetComponentInParent<SpaceManager>();

    if (current_space && row == current_space.row && col == current_space.col)
    {
        return;
    }

    //find the space corresponding to the given coordinates
    SpaceManager s = GetSpaceWithCoord(row, col);
    if (s) transform.SetParent(s.gameObject.transform);

    transform.localPosition = new Vector3(0, 0.1f, 0);
}
*/


    /*
    [Command]
    public void CmdSetLocation(int r, int c)
    {
        //find the space corresponding to the given coordinates
        foreach (SpaceManager s in spaces)
        {
            if (r == s.row && c == s.col)
            {
                row = r;
                col = c;

                //child the player on each client
                return;
            }
        }
    }
    */

    /*
    [Command]
    public void CmdGiveRecources(PlayerManager player)
    {
        CmdGiveRecources(player.gameObject.GetComponent<Inventory>());
    }

    [Command]
    public void CmdGiveRecources(Inventory player)
    {
        if (!isServer)
        {
            return;
        }

        if (!has_recources)
        {
            return;
        }

        if (player == owner || available_to_all)
        {
            player.CmdAddValue(3, (int)Inventory.Indexes.Grain);
        }
    }
    */
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

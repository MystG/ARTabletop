using System.Collections;
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

    public override void OnStartClient()
    {
        GetComponent<Collider>().enabled = false;
    }

    /*
    [Command]
    public void CmdSetOwner()
    {
        RpcSetOwner();
    }
    */

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

    /*
    [Command]
    public void CmdSetColor(float r, float g, float b)
    {
        RpcSetColor(r,g,b);
    }
    */

    [ClientRpc]
    public void RpcSetColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }
}

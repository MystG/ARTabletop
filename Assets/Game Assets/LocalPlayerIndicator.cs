using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocalPlayerIndicator : NetworkBehaviour
{
    private bool active = false;
    private PlayerManager local_player;

    public Vector3 local_position;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(!active)
        {
            return;
        }

    }

    [ClientRpc]
    public void RpcActivate()
    {
        active = true;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();

        foreach(PlayerManager p in players)
        {
            if(p.isLocalPlayer)
            {
                local_player = p;
                transform.SetParent(local_player.gameObject.transform);
                transform.localPosition = local_position;
                return;
            }
        }
    }
}

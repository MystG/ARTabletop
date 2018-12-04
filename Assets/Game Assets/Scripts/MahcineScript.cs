using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MachineScript : NetworkBehaviour
{
    public GameObject owner = null;

    [SyncVar]
    public bool available_to_all = false;

    [SyncVar]
    public bool has_recources = false;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}

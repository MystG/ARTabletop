using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpaceManager : NetworkBehaviour{

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool ValidMove(SpaceManager destination)
    {
        return true;
    }

    public bool GivesPlayerRecources(PlayerManagerScript player)
    {
        MachineScript mach = GetComponentInChildren<MachineScript>();

        if (!mach)
        {
            return false;
        }

        if(!mach.has_recources)
        {
            return false;
        }

        if(mach.available_to_all || mach.owner == player)
        {
            return true;
        }

        return false;
    }
}

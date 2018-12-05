using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpaceManager : NetworkBehaviour
{
    [SyncVar] public int row;
    [SyncVar] public int col;

    public int max_move_dist;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int DistanceTo(SpaceManager dest)
    {
        //return Mathf.Abs(row - destination.row) + Mathf.Abs(col - destination.col);

        int ax = col;
        int az = row - (col - (col & 1)) / 2;
        int ay = -ax - az;
        int bx = dest.col;
        int bz = dest.row - (dest.col - (dest.col & 1)) / 2;
        int by = -bx - bz;

        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) /2;
    }

    public bool ValidMove(SpaceManager destination, int E)
    {
        int dist = DistanceTo(destination);
        return dist <= max_move_dist && dist <= E;
    }

    /*
    public bool GivesPlayerRecources(PlayerManager player)
    {
        MachineScript mach = GetComponentInChildren<MachineScript>();

        if (!mach)
        {
            return false;
        }

        if (!mach.has_recources)
        {
            return false;
        }

        return mach.available_to_all || mach.owner == player;
    }
    */
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocationManager : NetworkBehaviour
{
    public Vector3 space_offset;

    private Dictionary<Vector2, SpaceManager> space_coords;

    [SyncVar] public int row;
    [SyncVar] public int col;

    /*
    // Use this for initialization
    void Start () {

        space_coords = new Dictionary<Vector2, SpaceManager>();
        SpaceManager[] spaces = FindObjectsOfType<SpaceManager>();

        foreach(SpaceManager s in spaces)
        {
            space_coords.Add(new Vector2(s.row, s.col), s);
        }
    }
    */

    public override void OnStartClient()
    {
        space_coords = new Dictionary<Vector2, SpaceManager>();
        SpaceManager[] spaces = FindObjectsOfType<SpaceManager>();

        foreach (SpaceManager s in spaces)
        {
            space_coords.Add(new Vector2(s.row, s.col), s);
        }
    }

    void Update()
    {
        /*
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
        */

        //RpcUpdateTransform(row, col);
    }

    [Command]
    public void CmdSetLocation(int r, int c)
    {
        row = r;
        col = c;

        RpcUpdateTransform(r, c);
    }

    [ClientRpc]
    private void RpcUpdateTransform(int r, int c)
    {
        row = r;
        col = c;

        SpaceManager current_space = Get_Space(r,c);

        if(current_space)
        {
            transform.SetParent(current_space.gameObject.transform);
        }

        transform.localPosition = space_offset;
    }

    public SpaceManager Get_Space(int r, int c)
    {
        Vector2 coords = new Vector2(r, c);
        if (space_coords.ContainsKey(coords))
        {
            return space_coords[coords];
        }
        else
        {
            return null;
        }
    }

    public SpaceManager CurrentSpace()
    {
        return Get_Space(row, col);
    }
}

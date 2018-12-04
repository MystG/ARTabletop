using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Inventory : NetworkBehaviour
{
    public enum Indexes { HP, Energy, Grain, Metal, Coins };

    private SyncListInt values;

    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        values = new SyncListInt
        {
            5, 20, 0, 0, 0
        };
    }
    
    public void SetValue(int val, int index)
    {
        if (!isServer)
            return;

        if(index < 0 || index >= values.Count)
        {
            return;
        }

        if(val < 0)
        {
            values[index] = 0;
        }
        else
        {
            values[index] = val;
        }
    }

    public void AddValue(int val, int index)
    {
        if (!isServer)
            return;

        if (index < 0 || index >= values.Count)
        {
            return;
        }

        values[index] -= val;

        if (values[index] < 0)
        {
            values[index] = 0;
        }
    }

    public int GetValue(int index)
    {
        return values[index];
    }

    private void UpdateInventoryUI()
    {

    }
}

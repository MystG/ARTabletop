using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Inventory : NetworkBehaviour
{
    public enum Indexes { HP, Energy, Grain, Metal, Coins };

    //private List<int> values;

    [SyncVar] private int HP = 5;
    [SyncVar] private int Energy = 20;
    [SyncVar] private int Grain = 0;
    [SyncVar] private int Metal = 0;
    [SyncVar] private int Coins = 0;

    private Text HPText;
    private Text EText;
    private Text GrainText;

    void Start()
    {
        /*
        values = new List<int>
        {
            5, 20, 0, 0, 0
        };
        */

        HPText = GameObject.Find("HP Text").GetComponent<Text>();
        EText = GameObject.Find("Energy Text").GetComponent<Text>();
        GrainText = GameObject.Find("Grain Text").GetComponent<Text>();
    }

    [Command]
    public void CmdSetValue(int val, int index)
    {
        if (!isServer)
            return;

        if (index < 0 || index > 2)
        {
            return;
        }

        if (val < 0)
        {
            val = 0;
        }

        switch (index)
        {
            case (int)Indexes.HP:
                HP = val;
                break;
            case (int)Indexes.Energy:
                Energy = val;
                break;
            case (int)Indexes.Grain:
                Grain = val;
                break;
        }

        RpcUpdateInventoryUI();
    }

    [Command]
    public void CmdAddValue(int val, int index)
    {
        if (!isServer)
            return;

        if (index < 0 || index > 2)
        {
            return;
        }

        switch (index)
        {
            case (int)Indexes.HP:
                HP += val;
                if (HP < 0) HP = 0;
                break;
            case (int)Indexes.Energy:
                Energy += val;
                if (Energy < 0) Energy = 0;
                break;
            case (int)Indexes.Grain:
                Grain += val;
                if (Grain < 0) Grain = 0;
                break;
        }

        RpcUpdateInventoryUI();
    }

    public int GetValue(int index)
    {
        switch (index)
        {
            case (int)Indexes.HP:
                return HP;
            case (int)Indexes.Energy:
                return Energy;
            case (int)Indexes.Grain:
                return Grain;
            default:
                return -1;
        }
    }

    [ClientRpc]
    private void RpcUpdateInventoryUI()
    {
        if (!isLocalPlayer)
            return;

        HPText.text = "HP: " + HP;
        EText.text = "Energy: " + Energy;
        GrainText.text = "Grain: " + Grain;
    }
}

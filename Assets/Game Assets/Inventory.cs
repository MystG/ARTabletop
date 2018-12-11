﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Inventory : NetworkBehaviour
{
    public enum Indexes { HP, Energy, Coins, Grain, Metal };

    private Image HPbar;
    private Image Ebar;

    //private List<int> values;

    [SyncVar] private int HP = 5;
    [SyncVar] private int Energy = 20;
    [SyncVar] private int Coins = 0;
    [SyncVar] private int Grain = 0;
    [SyncVar] private int Metal = 0;

    private Text HPText;
    private Text EText;
    private Text CoinText;

    public override void OnStartClient()
    {
        HPText = GameObject.Find("HP Text").GetComponent<Text>();
        EText = GameObject.Find("Energy Text").GetComponent<Text>();
        CoinText = GameObject.Find("Coin Text").GetComponent<Text>();

        HPbar = GameObject.Find("HP").GetComponent<Image>();
        Ebar = GameObject.Find("E").GetComponent<Image>();

        //UI: set recource counter displays to initial values
        //RpcUpdateInventoryUI(HP, Energy, Coins);

        /*HPText.text = "HP: " + HP;
        EText.text = "Energy: " + Energy;
        CoinText.text = "Coin: " + Coins;*/
        HPText.text = "" + HP;
        EText.text = "" + Energy;
        CoinText.text = "$" + Coins;
    }

    [Command]
    public void CmdSetValue(int val, int index)
    {
        if (!isServer)
            return;

        if (index < 0 || index > 4)
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
            case (int)Indexes.Coins:
                Coins = val;
                break;
            case (int)Indexes.Grain:
                Grain = val;
                break;
            case (int)Indexes.Metal:
                Metal = val;
                break;
        }

        RpcUpdateInventoryUI(HP, Energy, Coins);
    }

    [Command]
    public void CmdAddValue(int val, int index)
    {
        if (!isServer)
            return;

        if (index < 0 || index > 4)
        {
            return;
        }

        Debug.Log("Before Energy: " + Energy + " Coins: " + Coins);

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
            case (int)Indexes.Coins:
                Coins += val;
                if (Coins < 0) Coins = 0;
                break;
            case (int)Indexes.Grain:
                Grain += val;
                if (Grain < 0) Grain = 0;
                break;
            case (int)Indexes.Metal:
                Metal += val;
                if (Metal < 0) Metal = 0;
                break;
        }

        Debug.Log("After Energy: " + Energy + " Coins: " + Coins);

        RpcUpdateInventoryUI(HP, Energy, Coins);
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
            case (int)Indexes.Metal:
                return Metal;
            case (int)Indexes.Coins:
                return Coins;
            default:
                return -1;
        }
    }

    //update the UI
    [ClientRpc]
    
    public void RpcUpdateInventoryUI(int hp, int e, int c)
    {
        //UI: update ui to display the player's HP, Energy, and Coins
        if (!isLocalPlayer)
            return;
        HPText.text = "" + hp;
        HPbar.fillAmount = hp / 5f;
        EText.text = "" + e;
        Ebar.fillAmount = e / 20f;
        CoinText.text = "$" + c;
    }
}

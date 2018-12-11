using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBillboard : MonoBehaviour {

    private Inventory inv;
    private Text text;

    // Use this for initialization
    void Start () {
        inv = GetComponentInParent<Inventory>();
        text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        text.text = "HP:" + inv.GetValue((int)Inventory.Indexes.HP)
                    + " E:" + inv.GetValue((int)Inventory.Indexes.Energy)
                    + " Coins:" + inv.GetValue((int)Inventory.Indexes.Coins);
    }
}

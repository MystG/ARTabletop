using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBillboard : MonoBehaviour {

    private Inventory inv;
    private Text text;
    private Image hpb;

    public bool can_be_attacked = false;

    // Use this for initialization
    void Start () {
        inv = GetComponentInParent<Inventory>();
        text = GetComponent<Text>();

        hpb = GameObject.Find("HP").GetComponent<Image>();
     
    }
	
	// Update is called once per frame
	void Update () {

        
        if (inv.isLocalPlayer)
        {
            text.text = "";
        }
        else
        {
            text.text = inv.GetValue((int)Inventory.Indexes.Energy)+"E & "
                    + "$ " + inv.GetValue((int)Inventory.Indexes.Coins);

            int ohp = inv.GetValue((int)Inventory.Indexes.Energy);

            hpb.fillAmount = ohp / 5f;

            if (can_be_attacked)
            {
                text.text += "\nATK";
            }


        }

        

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBillboard : MonoBehaviour {

    private Inventory inv;
    private Text text;
    public Image hpb;

    public GameObject Hpbar;
    public GameObject ATK;
    public bool can_be_attacked = false;

    // Use this for initialization
    void Start () {
        inv = GetComponentInParent<Inventory>();
        text = GetComponent<Text>();

        
     
    }
	
	// Update is called once per frame
	void Update () {

        
        if (inv.isLocalPlayer)
        {
            text.text = "";
            Hpbar.SetActive(false);
        }
        else
        {
            text.text = inv.GetValue((int)Inventory.Indexes.Energy)+"E & "
                    + "$ " + inv.GetValue((int)Inventory.Indexes.Coins);

            int ohp = inv.GetValue((int)Inventory.Indexes.Energy);

            hpb.fillAmount = ohp / 5f;

            if (can_be_attacked)
            {
                ATK.SetActive(true);
            }


        }

        

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineUI : MonoBehaviour {

    private MachineScript ms;
    private Text text;
	// Use this for initialization
	void Start () {
        ms = GetComponentInParent<MachineScript>();
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        string info = "";

        if(ms.has_recources)
        {
            info += "Coins:" + ms.recource_amount;

            if(ms.available_to_all)
            {
                info += "!!!";
            }
        }

        text.text = info;
	}
}

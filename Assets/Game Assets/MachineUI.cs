using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineUI : MonoBehaviour {

    private MachineScript ms;
    private Text text;

    public GameObject coinA;
    public GameObject coinU;
    public GameObject NumberText;
	// Use this for initialization
	void Start () {
        ms = GetComponentInParent<MachineScript>();
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        string info = "";
        info += ":$" + ms.recource_amount;

        if (ms.has_recources)
        {
            coinU.SetActive(true);
            coinA.SetActive(false);
            NumberText.SetActive(true);

            if(ms.available_to_all)
            {
                coinA.SetActive(true);
                coinU.SetActive(false);
            }
        }
        else
        {
            NumberText.SetActive(false);
            coinA.SetActive(true);
            coinU.SetActive(false);
        }

        text.text = info;
	}
}

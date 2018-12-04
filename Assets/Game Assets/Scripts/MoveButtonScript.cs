using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MoveButtonScript : MonoBehaviour{

    // Update is called once per frame
    void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MovePlayer();
            }
        }
    }

    private void MovePlayer()
    {
        SpaceManager destination = transform.parent.GetComponent<SpaceManager>();
        if (!destination) return;

        PlayerManagerScript[] players = GameObject.FindObjectsOfType<PlayerManagerScript>();
        
        foreach (PlayerManagerScript pm in players)
        {
            if (pm.isLocalPlayer && pm.can_move)
            {
                GameObject.Find("MoveUI").SetActive(false);
                pm.can_move = false;
                pm.CmdMove(destination);
                return;
            }
        }
    }
}

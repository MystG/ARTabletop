using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButtonScript : MonoBehaviour {

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider == this.GetComponent<Collider>())
            {
                //Debug.Log(transform.parent.gameObject.name);
                MovePlayer();
            }
        }
    }

    private void MovePlayer()
    {
        SpaceManager destination = transform.parent.GetComponent<SpaceManager>();
        if (!destination) return;

        PlayerManager[] players = GameObject.FindObjectsOfType<PlayerManager>();
        
        foreach (PlayerManager pm in players)
        {
            if (pm.isLocalPlayer && pm.can_move)
            {
                //GameObject.Find("MoveUI").SetActive(false);
                pm.can_move = false;
                
                pm.CmdMove(destination.row, destination.col);

                break;
            }
        }

        MoveButtonScript[] buttons = GameObject.FindObjectsOfType<MoveButtonScript>();
        foreach (MoveButtonScript b in buttons)
        {
            b.gameObject.SetActive(false);
        }
    }
}

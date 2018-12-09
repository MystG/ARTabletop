using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

                MoveButtonScript[] buttons = GameObject.FindObjectsOfType<MoveButtonScript>();
                foreach (MoveButtonScript b in buttons)
                {
                    b.gameObject.SetActive(false);
                }

                GameObject.Find("Turn Text").GetComponent<Text>().text = "End your turn";
                
                pm.CmdMove(destination.row, destination.col);

                break;
            }
        }
    }
}

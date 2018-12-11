using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveButtonScript : MonoBehaviour {

    // Update is called once per frame
    void Update()
    {
        Vector3 pos;
        bool input_down = InputPos(out pos);

        if (input_down)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
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
                pm.ActivateMove(destination.row, destination.col);
                return;
            }
        }
    }

    public bool InputPos(out Vector3 result)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                result = touch.position;
                return true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            result = Input.mousePosition;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}

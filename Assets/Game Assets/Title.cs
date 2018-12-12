using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour 
{

    private void Update()
    {
        Vector3 pos;
        bool input_down = InputPos(out pos);
        if (input_down)
        {
            SceneManager.LoadScene(1);
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

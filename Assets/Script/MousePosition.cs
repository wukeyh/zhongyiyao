using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MousePosition
{

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f, -1))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
}


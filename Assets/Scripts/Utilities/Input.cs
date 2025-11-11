using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Input
    {
        public static Vector3 MouseToWorldPosition()
        {
            Vector3 worldPoint = Vector3.zero;
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane plane = new Plane(Vector3.up, Vector3.zero); //? Could be cached for better performance if needed
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                worldPoint = ray.GetPoint(distance);
            }
            return worldPoint;
        }
    }
}
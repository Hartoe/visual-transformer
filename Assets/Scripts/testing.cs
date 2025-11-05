using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    Grid grid;
    [SerializeField] Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(4, 2, 10f, new Vector3(0, 0));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = camera.transform.position.z;
            mousePos = camera.ScreenToWorldPoint(mousePos) * -1;
            grid.SetValue(mousePos, 10);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = camera.transform.position.z;
            mousePos = camera.ScreenToWorldPoint(mousePos) * -1;
            Debug.Log(grid.GetValue(mousePos));
        }
    }

}

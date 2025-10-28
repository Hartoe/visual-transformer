using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    Grid grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(4, 2, 10f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(Camera.main.ScreenToWorldPoint(Input.mousePosition), 10);
        }
    }

}

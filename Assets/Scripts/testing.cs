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
        grid = new Grid(10, 10, 10f, new Vector3(0, 0));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(Utilities.Input.MouseToWorldPosition(), 10);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.GetValue(Utilities.Input.MouseToWorldPosition()));
        }
    }

}

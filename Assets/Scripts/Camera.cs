using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    float movespeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Update this to use the new unity input system later!
        if (Input.GetKey(KeyCode.W)) { }
        if (Input.GetKey(KeyCode.A)) { }
        if (Input.GetKey(KeyCode.S)) { }
        if (Input.GetKey(KeyCode.D)) { }
    }
}

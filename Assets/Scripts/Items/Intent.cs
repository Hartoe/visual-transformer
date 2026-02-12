using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intent : WorldItem
{
    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public bool Moved;
    public Vector3 targetPosition { get; private set; }

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;
    }

    private void CheckMoved(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (Moved) Moved = false;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
    }

    public void MoveTo(Vector3 position)
    {
        SetTargetPosition(position);
        Moved = true;
    }

    void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }

    void OnDestroy()
    {
        TimeTickSystem.OnTick -= CheckMoved;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;

// TODO: Make this an abstract class that determines model and matrix shape (should this be an SO?)
public class WorldItem : MonoBehaviour, ICloneable
{
    public Matrix state;
    public bool Moved;
    public Vector3 targetPosition { get; protected set; }
    private float epsilon = 0.001f;
    private bool toBeDestroyed = false;

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;
    }

    protected void CheckMoved(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (Moved) Moved = false;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        if (toBeDestroyed)
        {
            if (Vector3.Distance(transform.position, targetPosition) <= epsilon)
                Destroy(gameObject);
        }
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        Moved = true;
    }

    public void DestroyOnArrival()
    {
        toBeDestroyed = true;
    }

    void OnDestroy()
    {
        TimeTickSystem.OnTick -= CheckMoved;
    }

    public object Clone()
    {
        WorldItem clone = Instantiate(this, transform.position, Quaternion.identity);
        clone.targetPosition = targetPosition;
        clone.state = state;
        clone.Moved = Moved;
        return clone;
    }
}

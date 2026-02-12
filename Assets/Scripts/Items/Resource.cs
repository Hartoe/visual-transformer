using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    IRON,
    WOOD,
    NAILS,
    CIRCUIT,
    PLASTIC,
    FABRIC,
    ORDER
}

public class Resource : WorldItem
{
    public static int ResourceCount = Enum.GetNames(typeof(ResourceType)).Length;

    public ResourceType resourceType;

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;

        double[,] shape = new double[1,ResourceCount];
        shape[0,(int)resourceType] = 1;
        state = new Utilities.ML.Matrix(shape);
    }
}

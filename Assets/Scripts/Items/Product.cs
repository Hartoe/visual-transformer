using System;
public enum ProductType
{
    CHAIR,
    COMPUTER,
    DRESS,
    FOOTBALL,
    DOOR,
    CANVAS,
    EOS
}

public class Product : WorldItem
{
    public static int ProductCount = Enum.GetNames(typeof(ProductType)).Length;

    public ProductType productType;

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;

        double[,] shape = new double[1,ProductCount];
        shape[0,(int)productType] = 1;
        state = new Utilities.ML.Matrix(shape);
    }
}

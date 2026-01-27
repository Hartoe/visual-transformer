using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;

public class MLTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FullyConnectedNN embedding = new FullyConnectedNN((4, 4), (4, 10), (4, 4));
        embedding.SetCostFunction(new Cost.MeanSquaredError());
        embedding.SetActivationFunction(new Activation.Sigmoid());
        PositionalEmbedding positionalEncoding = new PositionalEmbedding();
        LayerNorm layerNorm = new LayerNorm((4,4));
        Matrix inputs = new Matrix(new double[,]
        {
            {1, 0, 0, 0},
            {0, 0, 1, 0},
            {0, 1, 0, 0},
            {0, 0, 0, 1}
        });
        
        Matrix outputs = embedding.CalculateOutputs(inputs);
        Debug.Log(outputs);
        outputs = positionalEncoding.CalculateOutputs(outputs);
        Debug.Log(outputs);
        outputs = layerNorm.CalculateOutputs(outputs);
        Debug.Log(outputs);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using Utilities.ML;

public class MLTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        Layer test = new Layer((2, 3), (2, 4));

        Matrix inputs = new Matrix(new double[,]
        {
            {1, 2, 3},
            {4, 5, 6}
        });
        Matrix outputs = test.CalculateOutputs(inputs);
        Debug.Log(test.weights);
        Debug.Log(test.biases);
        Debug.Log(outputs);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

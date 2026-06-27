using System;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class JSONTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Encoder output (get from last level output)
        Matrix input = JSON.JSONToMatrix(JSON.LoadJSONFile("/lvl_4/output_matrix.json"));
        FullyConnectedNN network = new FullyConnectedNN((6,7),(6,12),(6,7));
        Activation.Softmax softmax = new Activation.Softmax();
        
        Debug.Log($"Input:\n{input}");
        Matrix networkMatrix = network.CalculateOutputs(input);
        Debug.Log($"Network:\n{networkMatrix}");
        Matrix lastSeq = new Matrix(1, networkMatrix.Columns);
        for (int i = 0; i < networkMatrix.Columns; i++)
            lastSeq[0,i] = networkMatrix[networkMatrix.Rows-1, i];
        Debug.Log($"Last Seq:\n{lastSeq}");
        Matrix output = new Matrix(lastSeq.Shape);
        for (int i = 0; i < output.Columns; i++)
        {
                output[0,i] = softmax.Activate(lastSeq, 0, i);
        }
        Debug.Log($"Output:\n{output}");

        // Get max from row
        double max = 0;
        int index = -1;
        for (int i = 0; i < output.Columns; i++)
        {
            if(output[0,i] > max)
            {
                max = output[0,i];
                index = i;
            }
        }

        Debug.Log(Enum.GetName(typeof(ProductType), (ProductType)index));

        // Save to JSON
        string networkJSON = JSON.NetworkToJSON(network);
        string outputJSON = JSON.MatrixToJSON(output);

        JSON.SaveJSONFile("/lvl_5/output_matrix.json", outputJSON);
        JSON.SaveJSONFile("/lvl_5/final_network_matrix.json", networkJSON);

    }
}

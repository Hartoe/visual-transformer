using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.ML;
using static Utilities.JSON;

public class JSONTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string jsonLoad = JSON.LoadJSONFile("/random_layer_values.json");
        NetworkJSON obj = JSON.JSONToNetwork(jsonLoad);
        FullyConnectedNN network = new FullyConnectedNN((7,7),(7,20),(7,7));
        int index = 0;
        foreach (LayerJSON layerJSON in obj.layerJSONs)
        {
            network.layers[index].SetWeights(layerJSON.weights.ToMatrix());
            network.layers[index].SetBiases(layerJSON.biases.ToMatrix());
            index++;
        }

        Debug.Log(network.layers[0].weights);
        Debug.Log(network.layers[0].biases);
    }
}

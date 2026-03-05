using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class Encoder : AFactory
{
    [SerializeField] Intent itemPrefab;
    [Header("Network Functions")]
    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [Header("Layer Sizes")]
    [SerializeField] int inputRows;
    [SerializeField] int inputColumns;
    [SerializeField] int hiddenRows;
    [SerializeField] int hiddenColumns;
    [SerializeField] int outputRows;
    [SerializeField] int outputColumns;
    [Header("JSON Paths")]
    [SerializeField] string attentionPath;
    [SerializeField] int level;
    [SerializeField] string networkPath;

    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Matrix> inputs = new List<Matrix>();
    
    private FullyConnectedNN network;
    private Attention attention;
    private LayerNorm layerNorm;
    
    new void Start()
    {
        attention = new Attention(inputRows, inputColumns);
        layerNorm = new LayerNorm((inputRows, inputColumns));
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));

        if (!string.IsNullOrEmpty(attentionPath))
        {
            string queryPath = $"/lvl_{level}/{attentionPath}_query_weights.json";
            string keyPath = $"/lvl_{level}/{attentionPath}_key_weights.json";
            string valuePath = $"/lvl_{level}/{attentionPath}_value_weights.json";

            JSON.LayerJSON queryJSON = JSON.JSONToLayer(JSON.LoadJSONFile(queryPath));
            JSON.LayerJSON keyJSON = JSON.JSONToLayer(JSON.LoadJSONFile(keyPath));
            JSON.LayerJSON valueJSON = JSON.JSONToLayer(JSON.LoadJSONFile(valuePath));

            attention.queryLayer.SetWeights(queryJSON.weights.ToMatrix());
            attention.queryLayer.SetBiases(queryJSON.biases.ToMatrix());
            attention.keyLayer.SetWeights(keyJSON.weights.ToMatrix());
            attention.keyLayer.SetBiases(keyJSON.biases.ToMatrix());
            attention.valueLayer.SetWeights(valueJSON.weights.ToMatrix());
            attention.valueLayer.SetBiases(valueJSON.biases.ToMatrix());
        }
        if (!string.IsNullOrEmpty(networkPath))
        {
            JSON.NetworkJSON networkJSON = JSON.JSONToNetwork(JSON.LoadJSONFile(networkPath));
            network.SetWeightsAndBiases(networkJSON);
        }

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Intent)
        {
            // Save the matrix of the item
            inputs.Add(item.state);
            item.MoveTo(Center);
            item.DestroyOnArrival();
        }

        Break("The wrong type of item was passed!");
        item.MoveTo(Center);
        item.DestroyOnArrival();
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        // Check if outputs list is empty, return null
        if (outputs.Count <= 0) return null;

        // if not pop first item
        WorldItem item = outputs.First();
        outputs.RemoveAt(0);

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            try
            {
                Matrix input = inputs.First();
                inputs.RemoveAt(0);
                
                attention.Queries = input;
                attention.Keys = input;
                attention.Values = input;
                Matrix attentionMatrix = attention.CalculateOutputs(input);
                Matrix middleNorm = layerNorm.CalculateOutputs(attentionMatrix + input);
                Matrix networkMatrix = network.CalculateOutputs(middleNorm);
                Matrix output = layerNorm.CalculateOutputs(networkMatrix + middleNorm);

                WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
                newItem.state = output;
                outputs.Add(newItem);
            }
            catch
            {
                Break("Wrong dimensions for encoder input!");
            }
        }
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX, cellY + 3));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 3, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 3));
                InputCells.Add((cellX + 1, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 3, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    protected override void Reset()
    {
        outputs = new List<WorldItem>();
        inputs = new List<Matrix>();
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class Decoder : AFactory
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
    [SerializeField] string decoderAttentionPath;
    [SerializeField] string mixedAttentionPath;
    [SerializeField] int level;
    [SerializeField] string networkPath;
    private Attention decoderAttention;
    private Attention mixedAttention;
    private FullyConnectedNN network;
    private LayerNorm layerNorm;

    private List<WorldItem> outputs = new List<WorldItem>();
    private Matrix decoderMatrix, encoderMatrix;
    private bool decoderSet = false;
    private bool encoderSet = false;

    new void Start()
    {
        decoderAttention = new Attention(inputRows, inputColumns);
        mixedAttention = new Attention(inputRows, inputColumns);
        layerNorm = new LayerNorm((inputRows, inputColumns));
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));

        if (!string.IsNullOrEmpty(decoderAttentionPath))
        {
            string queryPath = $"/lvl_{level}/{decoderAttentionPath}_query_weights.json";
            string keyPath = $"/lvl_{level}/{decoderAttentionPath}_key_weights.json";
            string valuePath = $"/lvl_{level}/{decoderAttentionPath}_value_weights.json";

            JSON.LayerJSON queryJSON = JSON.JSONToLayer(JSON.LoadJSONFile(queryPath));
            JSON.LayerJSON keyJSON = JSON.JSONToLayer(JSON.LoadJSONFile(keyPath));
            JSON.LayerJSON valueJSON = JSON.JSONToLayer(JSON.LoadJSONFile(valuePath));

            decoderAttention.queryLayer.SetWeights(queryJSON.weights.ToMatrix());
            decoderAttention.queryLayer.SetBiases(queryJSON.biases.ToMatrix());
            decoderAttention.keyLayer.SetWeights(keyJSON.weights.ToMatrix());
            decoderAttention.keyLayer.SetBiases(keyJSON.biases.ToMatrix());
            decoderAttention.valueLayer.SetWeights(valueJSON.weights.ToMatrix());
            decoderAttention.valueLayer.SetBiases(valueJSON.biases.ToMatrix());
        }
        if (!string.IsNullOrEmpty(mixedAttentionPath))
        {
            string queryPath = $"/lvl_{level}/{mixedAttentionPath}_query_weights.json";
            string keyPath = $"/lvl_{level}/{mixedAttentionPath}_key_weights.json";
            string valuePath = $"/lvl_{level}/{mixedAttentionPath}_value_weights.json";

            JSON.LayerJSON queryJSON = JSON.JSONToLayer(JSON.LoadJSONFile(queryPath));
            JSON.LayerJSON keyJSON = JSON.JSONToLayer(JSON.LoadJSONFile(keyPath));
            JSON.LayerJSON valueJSON = JSON.JSONToLayer(JSON.LoadJSONFile(valuePath));

            mixedAttention.queryLayer.SetWeights(queryJSON.weights.ToMatrix());
            mixedAttention.queryLayer.SetBiases(queryJSON.biases.ToMatrix());
            mixedAttention.keyLayer.SetWeights(keyJSON.weights.ToMatrix());
            mixedAttention.keyLayer.SetBiases(keyJSON.biases.ToMatrix());
            mixedAttention.valueLayer.SetWeights(valueJSON.weights.ToMatrix());
            mixedAttention.valueLayer.SetBiases(valueJSON.biases.ToMatrix());
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
        if (item is Intent)
        {
            if (cell == InputCells[0]) {
                // If A is null, set A and put the factory on occupied
                if (!decoderSet)
                {
                    decoderMatrix = item.state;
                    decoderSet = true;
                }
            } else if (cell == InputCells[1]) {
                // If B is null, set B and put the factory on occupied
                if (!encoderSet)
                {
                    encoderMatrix = item.state;
                    encoderSet = true;
                }
            }
            item.MoveTo(Center);
            item.DestroyOnArrival();
            return;
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
        if (decoderSet && encoderSet)
        {
            // decoder-only attention
            decoderAttention.Queries = decoderMatrix;
            decoderAttention.Keys = decoderMatrix;
            decoderAttention.Values = decoderMatrix;
            Matrix decoderAttentionMatrix;
            Matrix decoderAddNormMatrix;
            try
            {
                decoderAttentionMatrix = decoderAttention.CalculateOutputs(decoderMatrix);
                decoderAddNormMatrix = layerNorm.CalculateOutputs(decoderAttentionMatrix + decoderMatrix);
            }
            catch
            {
                Break("Wrong dimensions for decoder input!");
                return;
            }
            mixedAttention.Queries = decoderAddNormMatrix;
            mixedAttention.Values = encoderMatrix;
            mixedAttention.Keys = encoderMatrix;
            Matrix mixedAttentionMatrix, mixedAddNormMatrix, networkMatrix, output;
            try
            {
                mixedAttentionMatrix = mixedAttention.CalculateOutputs(decoderAddNormMatrix);
                mixedAddNormMatrix = layerNorm.CalculateOutputs(mixedAttentionMatrix + decoderAddNormMatrix);
                networkMatrix = network.CalculateOutputs(mixedAddNormMatrix);
                output = layerNorm.CalculateOutputs(networkMatrix + mixedAddNormMatrix);
            }
            catch
            {
                Break("Wrong dimensions for encoder component!");
                return;
            }
            WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);

            decoderSet = false;
            encoderSet = false;
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
                InputCells.Add((cellX + 2, cellY));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 3, cellY + 1));
                InputCells.Add((cellX, cellY -1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 3));
                InputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX - 1, cellY + 2));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 3, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 2, cellY + 2));
                break;
        }
    }

    public override bool Occupied((int, int) cell)
    {
        if (cell == InputCells[0]) return decoderSet || broken;
        if (cell == InputCells[1]) return encoderSet || broken;
        return broken;
    }

    protected override void Reset()
    {
        decoderSet = false;
        encoderSet = false;
        outputs = new List<WorldItem>();
    }
}

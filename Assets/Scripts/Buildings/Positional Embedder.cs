using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class PositionalEmbedder : PassThroughFactory
{
    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [SerializeField] Intent itemPrefab;
    [SerializeField] int inputRows = 7;
    [SerializeField] int inputColumns = 7;
    [SerializeField] int hiddenRows = 7;
    [SerializeField] int hiddenColumns = 20;
    [SerializeField] int outputRows = 7;
    [SerializeField] int outputColumns = 7;

    [Header("Weights and Biases")]
    [SerializeField] string JSONPath;
    private FullyConnectedNN network;
    private bool mustGenerate = false;
    private PositionalEmbedding positionalEmbedding = new PositionalEmbedding();

    new void Start()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetCostFunction(Cost.GetCostFromType(costType));

        if (!string.IsNullOrEmpty(JSONPath))
        {
            network.SetWeightsAndBiases(JSON.JSONToNetwork(JSON.LoadJSONFile(JSONPath)));
        }

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE or PRODUCT
        if (item is Resource || item is Product)
        {
            if (item is Resource)
            {
                if (((Resource)item).resourceType == ResourceType.ORDER)
                    mustGenerate = true;
            }
            inputs.Add(item.state);
            UpdateInfoPanel();
            item.MoveTo(Center);
            item.DestroyOnArrival();
            if (coroutineOnAction == null) StartAnimation();
            return;
        }

        Break("The wrong type of item was passed!");
        item.MoveTo(Center);
        item.DestroyOnArrival();
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (mustGenerate && inputs.Count != 0)
        {
            mustGenerate = false;
            double[,] tokenEmbeddingArray = new double[inputs.Count,Resource.ResourceCount];
            for (int i = 0; i < inputs.Count; i++)
            {
                for (int j = 0; j < Resource.ResourceCount; j++)
                {
                    tokenEmbeddingArray[i,j] = inputs[i][0,j];
                }
            }
            Matrix tokenEmbedding = new Matrix(tokenEmbeddingArray);

            try
            {
                tokenEmbedding = network.CalculateOutputs(tokenEmbedding);
                tokenEmbedding = positionalEmbedding.CalculateOutputs(tokenEmbedding);
            }
            catch
            {
                Break("Dimensions of the input were wrong!");
            }

            Intent newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = tokenEmbedding;
            outputs.Add(newItem);
            Invoke(buildingTypeSO.nameString, newItem.state);
            inputs.Clear();
            return;
        }
    }

    public override bool Occupied((int, int) cell)
    {
        return mustGenerate || broken || !InputCells.Contains(cell);
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX, cellY + 2));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 2, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 2));
                InputCells.Add((cellX + 1, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 2, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            NetworkInfoPanel panel = infoPanelInstance.GetComponent<NetworkInfoPanel>();
            if ((int)activationType != panel.activationDropdown.value)
                panel.activationDropdown.value = (int)activationType;
            if (panel.weightsDropdown != null)    
            {
                if (JSONPath != JSON.GetJSONPath(panel.weightsDropdown.options[panel.weightsDropdown.value].text, panel.level))
                {
                    var optionsList = panel.weightsDropdown.options.Select(option => option.text).ToList();
                    panel.weightsDropdown.value = optionsList.IndexOf(JSON.GetFileName(JSONPath));
                }
                panel.weightsDropdown.onValueChanged.AddListener(SetWeightsAndBiases);
            }

            panel.activationDropdown.onValueChanged.AddListener(SetActivationFunction);
        }
    }

    private void SetWeightsAndBiases(int arg0)
    {
        NetworkInfoPanel panel = infoPanelInstance.GetComponent<NetworkInfoPanel>();
        JSONPath = JSON.GetJSONPath(panel.weightsDropdown.options[arg0].text, panel.level);
        ReloadNetwork();
    }

    private void SetActivationFunction(int arg0)
    {
        activationType = (Activation.ActivationType)arg0;
        ReloadNetwork();
    }
    
    private void ReloadNetwork()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetWeightsAndBiases(JSON.JSONToNetwork(JSON.LoadJSONFile(JSONPath)));
    }

    protected override void Reset()
    {
        mustGenerate = false;
        base.Reset();
    }
}

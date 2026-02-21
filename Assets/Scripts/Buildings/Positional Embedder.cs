using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PositionalEmbedder : AFactory
{
    [Serializable]
    private struct Size
    {
        public int width, height;

        public (int, int) ToTuple() => (width, height);
    }

    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [SerializeField] Intent itemPrefab;
    [SerializeField] int inputRows = 7;
    [SerializeField] int inputColumns = 7;
    [SerializeField] int hiddenRows = 7;
    [SerializeField] int hiddenColumns = 20;
    [SerializeField] int outputRows = 7;
    [SerializeField] int outputColumns = 7;

    private FullyConnectedNN network;
    private List<Intent> outputs = new List<Intent>();
    private List<Resource> inputs = new List<Resource>();
    private bool mustGenerate = false;
    private PositionalEmbedding positionalEmbedding = new PositionalEmbedding();

    new void Start()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetCostFunction(Cost.GetCostFromType(costType));

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
         // Check if item is a RESOURCE
        if (item is Resource)
        {
            if (((Resource)item).resourceType == ResourceType.ORDER)
            {
                inputs.Add((Resource)item);
                mustGenerate = true;
            }
            else
            {
                inputs.Add((Resource)item);
            }
            UpdateInfoPanel();
        }

        //TODO: Handle wrong input with smoke effect and popup

        Destroy(item.gameObject);
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
        if (mustGenerate)
        {
            GenerateOutputItem();
            inputs.Clear();
            mustGenerate = false;
        }
    }

    private void GenerateOutputItem()
    {
        double[,] tokenEmbeddingArray = new double[inputs.Count,Resource.ResourceCount];
        for (int i = 0; i < inputs.Count; i++)
        {
            for (int j = 0; j < Resource.ResourceCount; j++)
            {
                tokenEmbeddingArray[i,j] = inputs[i].state[0,j];
            }
        }
        Matrix tokenEmbedding = new Matrix(tokenEmbeddingArray);

        tokenEmbedding = network.CalculateOutputs(tokenEmbedding);
        tokenEmbedding = positionalEmbedding.CalculateOutputs(tokenEmbedding);

        Intent newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
        newItem.state = tokenEmbedding;

        outputs.Add(newItem);
    }

    public override bool Occupied((int, int) cell)
    {
        return mustGenerate;
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
            if ((int)costType != panel.costDropdown.value)
                panel.costDropdown.value = (int)costType;
            if ((int)activationType != panel.activationDropdown.value)
                panel.activationDropdown.value = (int)activationType;
            if (inputRows != int.Parse(panel.inputRows.text))
                panel.inputRows.text = inputRows.ToString();
            if (inputColumns != int.Parse(panel.inputColumns.text))
                panel.inputColumns.text = inputColumns.ToString();
            if (hiddenRows != int.Parse(panel.hiddenRows.text))
                panel.hiddenRows.text = hiddenRows.ToString();
            if (hiddenColumns != int.Parse(panel.hiddenColumns.text))
                panel.hiddenColumns.text = hiddenColumns.ToString();
            if (outputRows != int.Parse(panel.outputRows.text))
                panel.outputRows.text = outputRows.ToString();
            if (outputColumns != int.Parse(panel.outputColumns.text))
                panel.outputColumns.text = outputColumns.ToString();

            panel.costDropdown.onValueChanged.AddListener(SetCostFunction);
            panel.activationDropdown.onValueChanged.AddListener(SetActivationFunction);
            panel.inputRows.onValueChanged.AddListener(SetInputRows);
            panel.inputColumns.onValueChanged.AddListener(SetInputColumns);
            panel.hiddenRows.onValueChanged.AddListener(SetHiddenRows);
            panel.hiddenColumns.onValueChanged.AddListener(SetHiddenColumns);
            panel.outputRows.onValueChanged.AddListener(SetOutputRows);
            panel.outputColumns.onValueChanged.AddListener(SetOutputColumns);
        }
    }

    private void SetOutputColumns(string arg0)
    {
        outputColumns = int.Parse(arg0);
        if (outputColumns <= 0) outputColumns = 1;
        ReloadNetwork();
    }

    private void SetOutputRows(string arg0)
    {
        outputRows = int.Parse(arg0);
        if (outputRows <= 0) outputRows = 1;
        ReloadNetwork();
    }

    private void SetHiddenColumns(string arg0)
    {
        hiddenColumns = int.Parse(arg0);
        if (hiddenColumns <= 0) hiddenColumns = 1;
        ReloadNetwork();
    }

    private void SetHiddenRows(string arg0)
    {
        hiddenRows = int.Parse(arg0);
        if (hiddenRows <= 0) hiddenRows = 1;
        ReloadNetwork();
    }

    private void SetInputColumns(string arg0)
    {
        inputColumns = int.Parse(arg0);
        if (inputColumns <= 0) inputColumns = 1;
        ReloadNetwork();
    }

    private void SetInputRows(string arg0)
    {
        inputRows = int.Parse(arg0);
        if (inputRows <= 0) inputRows = 1;
        ReloadNetwork();
    }

    private void SetActivationFunction(int arg0)
    {
        activationType = (Activation.ActivationType)arg0;
        ReloadNetwork();
    }

    private void SetCostFunction(int arg0)
    {
        costType = (Cost.CostType)arg0;
        ReloadNetwork();
    }
 
    private void ReloadNetwork()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        //TODO: Hotload weights and biases
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

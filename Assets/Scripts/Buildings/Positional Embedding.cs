using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PositionalEmbeddingNode : AFactory
{
    [Serializable]
    private struct Size
    {
        public int width, height;

        public (int, int) ToTuple() => (width, height);
    }

    [SerializeField] Size[] layerSizes;
    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [SerializeField] Intent itemPrefab;

    private FullyConnectedNN network;
    private List<Intent> outputs = new List<Intent>();
    private List<Resource> inputs = new List<Resource>();
    private bool mustGenerate = false;
    private PositionalEmbedding positionalEmbedding = new PositionalEmbedding();

    new void Start()
    {
        (int, int)[] layers = new (int,int)[layerSizes.Length];
        for (int i = 0; i < layerSizes.Length; i++) layers[i] = layerSizes[i].ToTuple();
        network = new FullyConnectedNN(layers);
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
        //TODO: CREATE THIS METHOD
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

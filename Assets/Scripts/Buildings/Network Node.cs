using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class NetworkNode : AFactory
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
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Matrix> inputs = new List<Matrix>();

    new void Start()
    {
        (int, int)[] layers = new (int,int)[layerSizes.Length];
        for (int i = 0; i < layerSizes.Length; i++) layers[i] = layerSizes[i].ToTuple();
        network = new FullyConnectedNN(layers);
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetCostFunction(Cost.GetCostFromType(costType));

        //TODO: Find a way to pass pre-trained weights and biases

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Intent)
        {
            // Save the matrix of the item
            inputs.Add(item.state);
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
        if (inputs.Count > 0)
        {
            try
            {
                Matrix input = inputs.First();
                inputs.RemoveAt(0);
                Matrix output = network.CalculateOutputs(input);
                WorldItem newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
                newItem.state = output;
                outputs.Add(newItem);
            }
            catch
            {
                //TODO: Handle wrong dimensions of input
                Debug.Log("Wrong dimensions of input");
            }

        }
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 1, cellY));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

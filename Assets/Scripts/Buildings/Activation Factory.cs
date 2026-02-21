using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class ActivationFactory : AFactory
{
    [SerializeField] Intent itemPrefab;
    public Activation.ActivationType activationType;
    private IActivation activationFunction;
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Matrix> inputs = new List<Matrix>();

    new void Start()
    {
        activationFunction = Activation.GetActivationFromType(activationType);
        base.Start();
    }
    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is INTENT
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
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = new Matrix(input.Shape);
            for (int i = 0; i < output.Rows; i++)
            {
                for (int j = 0; j < output.Columns; j++)
                {
                    output[i,j] = activationFunction.Activate(input, i, j);
                }
            }
            WorldItem newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
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

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            ActivationInfoPanel panel = infoPanelInstance.GetComponent<ActivationInfoPanel>();
            if ((int)activationType != panel.Dropdown.value)
                panel.Dropdown.value = (int)activationType;

            panel.Dropdown.onValueChanged.AddListener(ChangeActivatorFunction);
        }
    }

    private void ChangeActivatorFunction(int value)
    {
        activationType = (Activation.ActivationType)value;
        activationFunction = Activation.GetActivationFromType(activationType);
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

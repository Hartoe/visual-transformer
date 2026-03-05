using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PositionalFactory : AFactory
{
    [SerializeField] Intent itemPrefab;
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Matrix> inputs = new List<Matrix>();

    private PositionalEmbedding positionalEmbedding = new PositionalEmbedding();

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Intent)
        {
            // Save the matrix of the item
            inputs.Add(item.state);
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
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = positionalEmbedding.CalculateOutputs(input);
            WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
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

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }

    protected override void Reset()
    {
        inputs = new List<Matrix>();
        outputs = new List<WorldItem>();
    }
}

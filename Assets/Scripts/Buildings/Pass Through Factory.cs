using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PassThroughFactory : AFactory
{
    protected List<WorldItem> outputs = new List<WorldItem>();
    protected List<Matrix> inputs = new List<Matrix>();
    
    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is an INTENT
        if (item is Intent)
        {
            // Save the matrix of the item
            inputs.Add(item.state);
            item.MoveTo(Center);
            item.DestroyOnArrival();
            if (coroutineOnAction == null) StartAnimation();
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

        if (outputs.Count <= 0) StopAnimation();

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e) {}

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

    protected override void Reset()
    {
        outputs.Clear();
        inputs.Clear();
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            if (item != null) Destroy(item.gameObject);
        base.OnDestroy();
    }
}

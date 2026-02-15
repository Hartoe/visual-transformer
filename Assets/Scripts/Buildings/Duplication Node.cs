using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Duplicator : AFactory
{
    List<(int, WorldItem)> outputs = new List<(int, WorldItem)>();
    private WorldItem itemToDuplicate;
    private bool mustDuplicate = false;

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        itemToDuplicate = item;
        mustDuplicate = true;
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        if (outputs.Count <= 0) return null;

        // Get index for cell from output cells
        int index = OutputCells.IndexOf(cell);

        // Search for index in outputs list
        (_, WorldItem item) = outputs.Find((kvp) => kvp.Item1 == index);
        outputs.Remove((index, item));

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (mustDuplicate)
        {
            WorldItem duplicate = (WorldItem)itemToDuplicate.Clone();
            outputs.Add((1, itemToDuplicate));
            outputs.Add((0, duplicate));
            mustDuplicate = false;
        }
    }

    public override bool Occupied((int, int) cell)
    {
        return mustDuplicate;
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX, cellY - 1));
                OutputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                OutputCells.Add((cellX - 1, cellY + 1));
                InputCells.Add((cellX + 1, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                OutputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX + 1, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                OutputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    protected new void OnDestroy()
    {
        foreach(var kvp in outputs)
        {
            Destroy(kvp.Item2);
        }
        Destroy(itemToDuplicate);
        base.OnDestroy();
    }
}

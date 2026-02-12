using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class Embedder : AFactory
{
    [SerializeField] Intent itemPrefab;
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Resource> inputs = new List<Resource>();

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Resource)
        {
            if (((Resource)item).resourceType == ResourceType.ORDER)
            {
                inputs.Add((Resource)item);
                GenerateOutputItem();
                inputs.Clear();
            }
            else
            {
                inputs.Add((Resource)item);
            }
        }

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

    private void GenerateOutputItem()
    {
        // Generate the final matrix
        double[,] finalMatrix = new double[inputs.Count,Resource.ResourceCount];
        for (int i = 0; i < inputs.Count; i++)
        {
            for (int j = 0; j < Resource.ResourceCount; j++)
            {
                finalMatrix[i,j] = inputs[i].state[0,j];
            }
        }
        Matrix finalState = new Matrix(finalMatrix);

        Debug.Log(finalState);

        Intent newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
        newItem.state = finalState;

        outputs.Add(newItem);
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
}

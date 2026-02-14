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
    private bool mustGenerate = false;

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

        Intent newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
        newItem.state = finalState;

        outputs.Add(newItem);
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

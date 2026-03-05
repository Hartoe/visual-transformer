using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class EncodingFactory : AFactory
{
    [SerializeField] Intent itemPrefab;
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<WorldItem> inputs = new List<WorldItem>();
    private bool mustGenerate = false;

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Resource || item is Product)
        {
            if (item is Resource)
            {
                if (((Resource)item).resourceType == ResourceType.ORDER)
                    mustGenerate = true;
            }
            inputs.Add(item);
            UpdateInfoPanel();
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

        Intent newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
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
        return mustGenerate || broken;
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
            infoPanelInstance.GetComponent<EncodingInfoPanel>().ResetText();
            if (inputs.Count > 0)
                infoPanelInstance.GetComponent<EncodingInfoPanel>().SetText(inputs.ToArray());
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
        mustGenerate = false;
        outputs = new List<WorldItem>();
        inputs = new List<WorldItem>();
    }
}

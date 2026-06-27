using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class EncodingFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;
    private bool mustGenerate = false;

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE or PRODUCT
        if (item is Resource || item is Product)
        {
            if (item is Resource)
            {
                if (((Resource)item).resourceType == ResourceType.ORDER)
                    mustGenerate = true;
            }
            inputs.Add(item.state);
            UpdateInfoPanel();
            item.MoveTo(Center);
            item.DestroyOnArrival();
            if (coroutineOnAction == null) StartAnimation();
            return;
        }

        Break("The wrong type of item was passed!");
        item.MoveTo(Center);
        item.DestroyOnArrival();
    }

    private void GenerateOutputItem()
    {
        // Generate the final matrix
        double[,] finalMatrix = new double[inputs.Count,Resource.ResourceCount];
        for (int i = 0; i < inputs.Count; i++)
        {
            for (int j = 0; j < Resource.ResourceCount; j++)
            {
                finalMatrix[i,j] = inputs[i][0,j];
            }
        }
        Matrix finalState = new Matrix(finalMatrix);

        Intent newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
        newItem.state = finalState;

        outputs.Add(newItem);
        Invoke(buildingTypeSO.nameString, newItem.state);
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
        return mustGenerate || broken || !InputCells.Contains(cell);
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

    protected override void Reset()
    {
        mustGenerate = false;
        base.Reset();
    }
}

using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;
public class DuplicationFactory : AFactory
{
    List<(int, WorldItem)> outputs = new List<(int, WorldItem)>();
    [SerializeField] Intent itemToDuplicate;
    private Matrix itemState;
    private bool mustDuplicate = false;

    new void Start()
    {
        TimeTickSystem.OnTick += UpdateInfoPanel;
        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is INTENT
        if (item is Intent)
        {
            // Save the matrix of the item
            itemState = item.state;
            item.MoveTo(Center);
            item.DestroyOnArrival();
            mustDuplicate = true;
            if (coroutineOnAction == null) StartAnimation();
            return;
        }

        Break("The wrong type of item was passed!");
        item.MoveTo(Center);
        item.DestroyOnArrival();
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        if (outputs.Count <= 0) return null;

        // Get index for cell from output cells
        int index = OutputCells.IndexOf(cell);

        // Search for index in outputs list
        (_, WorldItem item) = outputs.Find((kvp) => kvp.Item1 == index);
        outputs.Remove((index, item));

        if (outputs.Count <= 0) StopAnimation();

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (mustDuplicate)
        {
            Intent item1 = Instantiate(itemToDuplicate, Center, Quaternion.identity);
            item1.state = itemState;
            Intent item2 = Instantiate(itemToDuplicate, Center, Quaternion.identity);
            item2.state = itemState;

            outputs.Add((1, item1));
            outputs.Add((0, item2));
            mustDuplicate = false;
            Invoke(buildingTypeSO.nameString, item1.state);
        }
    }

    public override bool Occupied((int, int) cell)
    {
        return mustDuplicate || broken || !InputCells.Contains(cell);
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

    private void UpdateInfoPanel(object sender, TimeTickSystem.TickEventArgs e)
    {
        UpdateInfoPanel();
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            if (itemToDuplicate != null)
                infoPanelInstance.GetComponent<PassInfoPanel>().SetText(itemToDuplicate);
            else infoPanelInstance.GetComponent<PassInfoPanel>().SetText(null);
        }
    }

    protected new void OnDestroy()
    {
        foreach(var kvp in outputs)
        {
            Destroy(kvp.Item2);
        }
        TimeTickSystem.OnTick -= UpdateInfoPanel;
        base.OnDestroy();
    }

    protected override void Reset()
    {
        mustDuplicate = false;
        outputs = new List<(int, WorldItem)>();
    }
}

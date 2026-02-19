using System;
using System.Collections.Generic;
using UnityEngine;

public class Generator : AFactory
{
    List<(int, WorldItem)> outputs = new List<(int, WorldItem)>();

    [SerializeField] List<WorldItem> itemPrefabs;
    [SerializeField] int amount;
    int currentItem = 0;
    string status = "Waiting";

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

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                break;
        }
    }    

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        GridBuildingSystem.GridObject outputCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(OutputCells[0].Item1, OutputCells[0].Item2);
        if (outputCell != null)
        {
            Building building = outputCell.GetBuilding();
            if (building != null)
            {
                if (building.GetBuildingTypeSO().nameString == "Conveyor" && amount > 0)
                {
                    // Create new world item
                    WorldItem newItem = Instantiate(itemPrefabs[currentItem], GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
                    outputs.Add((0, newItem));
                    currentItem++;
                    if (currentItem >= itemPrefabs.Count)
                    {
                        amount--;
                        currentItem = 0;
                    }
                    status = "Generating";
                    UpdateInfoPanel();
                    return;
                }
            }
        }

        if (amount <= 0)
            status = "Done";
        else
            status = "Waiting";
        UpdateInfoPanel();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            GeneratorInfoPanel panel = infoPanelInstance.GetComponent<GeneratorInfoPanel>();
            panel.SetStatus(status);

            if (panel.AddListener)
            {
                panel.Button.onClick.AddListener(ResetGeneration);
                panel.AddListener = false;
            }
        }
    }

    private void ResetGeneration()
    {
        currentItem = 0;
        amount = 1;
        status = "Waiting";
        UpdateInfoPanel();
    }

    new void OnDestroy()
    {
        foreach ((int, WorldItem) item in outputs)
            Destroy(item.Item2.gameObject);
        base.OnDestroy();
    }
}

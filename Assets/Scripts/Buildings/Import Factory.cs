using System.Collections.Generic;
using UnityEngine;

public class ImportFactory : PassThroughFactory
{
    [SerializeField] List<WorldItem> itemPrefabs;
    [SerializeField] int amount;
    int currentItem = 0;
    string status = "Waiting";

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
                    WorldItem newItem = Instantiate(itemPrefabs[currentItem], Center, Quaternion.identity);
                    outputs.Add(newItem);
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

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            ImportInfoPanel panel = infoPanelInstance.GetComponent<ImportInfoPanel>();
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
        Reset();
        UpdateInfoPanel();
    }

    protected override void Reset()
    {
        currentItem = 0;
        amount = 1;
        status = "Waiting";
        base.Reset();
    }
}

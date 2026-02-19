using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Conveyor : Building
{
    // Item that is currently on the conveyor
    WorldItem worldItem;
    int cellX, cellY;

    // Conveyor neighbourhood
    GridBuildingSystem.GridObject leftCell, rightCell, upCell, downCell;
    bool[] surroundingDirs = new bool[4];

#region Constructor
    void Start()
    {
        TimeTickSystem.OnTick += MoveWorldItem;
        TimeTickSystem.OnTick += UpdateInfoPanel;

        // Get current conveyor grid position
        int x, y;
        GridBuildingSystem.Instance.GetGrid().GetXY(transform.position, out x, out y);
        Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
        cellX = x - rotationOffset.x;
        cellY = y - rotationOffset.y;

        // Get neighbourhood cells
        leftCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX - 1, cellY);
        rightCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX + 1, cellY);
        upCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY + 1);
        downCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY - 1);

        UpdateConveyorModel();
    }
#endregion

#region Update Conveyor Model
    public void UpdateConveyorModel(int depth = 0)
    {
        if (depth > 1) return;

        // Get each surrounding cells building
        surroundingDirs[0] = CheckSurroundingDir(downCell, BuildingTypeSO.Dir.Up);
        surroundingDirs[1] = CheckSurroundingDir(leftCell, BuildingTypeSO.Dir.Right);
        surroundingDirs[2] = CheckSurroundingDir(upCell, BuildingTypeSO.Dir.Down);
        surroundingDirs[3] = CheckSurroundingDir(rightCell, BuildingTypeSO.Dir.Left);

        // Update current conveyor model
        List<string> names = new List<string>();
        switch (dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:        
                if (surroundingDirs[1]) names.Add("CurveRight");
                if (surroundingDirs[2]) names.Add("Straight");
                if (surroundingDirs[3]) names.Add("CurveLeft");
                if (downCell != null) // Check if flowing into a Conveyor
                    UpdateOtherConveyor(downCell, BuildingTypeSO.Dir.Up, depth + 1);
                break;
            case BuildingTypeSO.Dir.Up:
                if (surroundingDirs[0]) names.Add("Straight");
                if (surroundingDirs[1]) names.Add("CurveLeft");
                if (surroundingDirs[3]) names.Add("CurveRight");
                if (upCell != null) // Check if flowing into a Conveyor
                    UpdateOtherConveyor(upCell, BuildingTypeSO.Dir.Down, depth + 1);
                break;
            case BuildingTypeSO.Dir.Left:
                if (surroundingDirs[0]) names.Add("CurveLeft");
                if (surroundingDirs[2]) names.Add("CurveRight");
                if (surroundingDirs[3]) names.Add("Straight");
                if (leftCell != null) // Check if flowing into a Conveyor
                    UpdateOtherConveyor(leftCell, BuildingTypeSO.Dir.Right, depth + 1);
                break;
            case BuildingTypeSO.Dir.Right:
                if (surroundingDirs[0]) names.Add("CurveRight");
                if (surroundingDirs[1]) names.Add("Straight");
                if (surroundingDirs[2]) names.Add("CurveLeft");
                if (rightCell != null) // Check if flowing into a Conveyor
                    UpdateOtherConveyor(rightCell, BuildingTypeSO.Dir.Left, depth + 1);
                break;
        }
        if (names.Count == 0) names.Add("Straight");
        SetModelDir(names.ToArray());

    }

    private bool CheckSurroundingDir(GridBuildingSystem.GridObject cell, BuildingTypeSO.Dir cellDir)
    {
        // Check if cell is not null
        if (cell == null)
            return false;

        // Check if cell has a building
        Building building = cell.GetBuilding();
        if (building == null)
            return false;

        // Check the output direction of the building and check if it aligns with the conveyor
        if (building.GetBuildingTypeSO().nameString == "Conveyor") // It is a conveyor building
        {
            if (building.GetDir() == cellDir)
                return true;
            return false;
        }
        else // It is a factory building
        {
            return ((AFactory)building).OutputCells.Contains((cellX, cellY));
        }
    }

    private void SetModelDir(params string[] names)
    {
        foreach (Transform child in gameObject.GetComponentInChildren<Transform>(includeInactive: true))
        {
            if (names.Contains(child.gameObject.name))
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }
    }

    private void UpdateOtherConveyor(GridBuildingSystem.GridObject cell, BuildingTypeSO.Dir cellDir, int depth)
    {
        Building building = cell.GetBuilding();
        if (building != null)
        {
            if (building.GetBuildingTypeSO().nameString == "Conveyor" && building.GetDir() != cellDir)
                ((Conveyor)building).UpdateConveyorModel(depth);
        }
    }
#endregion

#region Update World Item
    private void MoveWorldItem(object sender, TimeTickSystem.TickEventArgs e)
    {
        // Check if the conveyor belt has an item it contains
        if (Occupied())
        {
            // Check if the item hasn't already moved this update
            if (!worldItem.Moved)
            {
                // Check which cell is the next cell
                GridBuildingSystem.GridObject nextCell;
                switch (dir)
                {
                    default:
                    case BuildingTypeSO.Dir.Down:
                        nextCell = downCell;
                        break;
                    case BuildingTypeSO.Dir.Left:
                        nextCell = leftCell;
                        break;
                    case BuildingTypeSO.Dir.Up:
                        nextCell = upCell;
                        break;
                    case BuildingTypeSO.Dir.Right:
                        nextCell = rightCell;
                        break;
                }

                if (nextCell != null)
                {
                    Building nextBuilding = nextCell.GetBuilding();
                    if (nextBuilding != null)
                    {
                        if (nextBuilding.GetBuildingTypeSO().nameString == "Conveyor")
                        {
                            if (!((Conveyor)nextBuilding).Occupied())
                            {
                                ((Conveyor)nextBuilding).SetItem(worldItem);
                                worldItem.MoveTo(nextCell.Center());
                                SetItem(null);
                            }
                        }
                        else if (((AFactory)nextBuilding).InputCells.Contains((cellX, cellY)))
                        {
                            if (!((AFactory)nextBuilding).Occupied((cellX, cellY)))
                            {
                                ((AFactory)nextBuilding).AddFromInput(worldItem, (cellX, cellY));
                                worldItem.MoveTo(nextCell.Center());
                                SetItem(null);
                            }
                        }
                    }
                }
            }
        }

        // Check if the conveyor is ready to receive a new item
        if (!Occupied())
        {
            List<GridBuildingSystem.GridObject> inputCells = new List<GridBuildingSystem.GridObject>();
            switch (dir)
            {
                default:
                case BuildingTypeSO.Dir.Down:        
                    inputCells.AddRange(new GridBuildingSystem.GridObject[3]{leftCell, rightCell, upCell});
                    break;
                case BuildingTypeSO.Dir.Up:
                    inputCells.AddRange(new GridBuildingSystem.GridObject[3]{leftCell, rightCell, downCell});
                    break;
                case BuildingTypeSO.Dir.Left:
                    inputCells.AddRange(new GridBuildingSystem.GridObject[3]{downCell, rightCell, upCell});
                    break;
                case BuildingTypeSO.Dir.Right:
                    inputCells.AddRange(new GridBuildingSystem.GridObject[3]{leftCell, downCell, upCell});
                    break;
            }

            foreach (GridBuildingSystem.GridObject cell in inputCells)
            {
                // Get the cell building
                if (cell != null)
                {
                    Building building = cell.GetBuilding();
                    if (building != null)
                    {
                        if (building.GetBuildingTypeSO().nameString != "Conveyor") // Conveyors pass but dont poll
                        {
                            if (((AFactory)building).OutputCells.Contains((cellX, cellY)))
                            {
                                WorldItem newItem = ((AFactory)building).RemoveFromOutput((cellX, cellY));
                                if (newItem != null)
                                {
                                    SetItem(newItem);
                                    worldItem.MoveTo(GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center());
                                }
                            }
                        } 
                    }
                }
            }
        }

    }
#endregion

#region Helper Functions
    public bool Occupied() => worldItem != null;

    private void UpdateInfoPanel(object sender, TimeTickSystem.TickEventArgs e)
    {
        UpdateInfoPanel();
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            if (worldItem != null)
                infoPanelInstance.GetComponent<PassInfoPanel>().SetText(worldItem);
            else infoPanelInstance.GetComponent<PassInfoPanel>().SetText(null);
        }
    }

    public void SetItem(WorldItem item)
    {
        worldItem = item;
        UpdateInfoPanel();
    }

    public WorldItem GetItem()
    {
        return worldItem;
    }

    private Vector2Int GetDirectionVector(BuildingTypeSO.Dir direction)
    {
        switch (dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                return new Vector2Int(0, -1);
            case BuildingTypeSO.Dir.Left:
                return new Vector2Int(-1, 0);
            case BuildingTypeSO.Dir.Up:
                return new Vector2Int(0, 1);
            case BuildingTypeSO.Dir.Right:
                return new Vector2Int(1, 0);
        }
    }
#endregion
    void OnDestroy()
    {
        TimeTickSystem.OnTick -= MoveWorldItem;
        TimeTickSystem.OnTick -= UpdateInfoPanel;
        if (worldItem != null) Destroy(worldItem.gameObject);
    }
}
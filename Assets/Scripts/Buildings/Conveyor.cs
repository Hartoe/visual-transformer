using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Conveyor : Building
{
    public bool occupiedAtStart, reserved;

    // Item that is currently on the conveyor
    public WorldItem currentItem, nextItem;
    public GridBuildingSystem.GridObject NextCell;
    public GridBuildingSystem.GridObject[] InputCells;
    int cellX, cellY;

    // Conveyor neighbourhood
    GridBuildingSystem.GridObject leftCell, rightCell, upCell, downCell;
    bool[] surroundingDirs = new bool[4];

#region Constructor
    void Start()
    {
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

        // Set relation of the surrounding cells
        NextCell = GetNextCell();
        InputCells = GetInputCells();

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
        else if (building is AFactory)
        {
            if (((AFactory)building).OutputCells != null)
                return ((AFactory)building).OutputCells.Contains((cellX, cellY));
            return false;
        }
        return false;
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

    #region Helper Functions
    public (int, int) CellPosition() => (cellX, cellY);

    private GridBuildingSystem.GridObject GetNextCell()
    {
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
        return nextCell;
    }
    private GridBuildingSystem.GridObject[] GetInputCells()
    {
        switch (dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:        
                return new GridBuildingSystem.GridObject[3]{leftCell, rightCell, upCell};
            case BuildingTypeSO.Dir.Up:
                return new GridBuildingSystem.GridObject[3]{leftCell, rightCell, downCell};
            case BuildingTypeSO.Dir.Left:
                return new GridBuildingSystem.GridObject[3]{downCell, rightCell, upCell};
            case BuildingTypeSO.Dir.Right:
                return new GridBuildingSystem.GridObject[3]{leftCell, downCell, upCell};
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
            if (currentItem != null)
                infoPanelInstance.GetComponent<PassInfoPanel>().SetText(currentItem);
            else infoPanelInstance.GetComponent<PassInfoPanel>().SetText(null);
        }
    }

    public void SetItem(WorldItem item)
    {
        currentItem = item;
        if (item != null) currentItem.MoveTo(GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center());
        UpdateInfoPanel();
    }

#endregion
    void OnDestroy()
    {
        TimeTickSystem.OnTick -= UpdateInfoPanel;
        if (currentItem != null) Destroy(currentItem.gameObject);
    }
}
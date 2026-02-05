using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conveyor : Building
{
    //! For Testing
    [SerializeField] WorldItem prefab;

    WorldItem worldItem;

    void Start()
    {
        TimeTickSystem.OnTick += MoveWorldItem;

        int x, y;
        GridBuildingSystem.Instance.GetGrid().GetXY(transform.position, out x, out y);
        Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
        int currentX = x - rotationOffset.x;
        int currentY = y - rotationOffset.y;
        
        Building leftCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(currentX - 1, currentY).GetBuilding();
        Building rightCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(currentX + 1, currentY).GetBuilding();
        Building upCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(currentX, currentY + 1).GetBuilding();
        Building downCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(currentX, currentY - 1).GetBuilding();

        bool[] surroundingDirs = new bool[4];
        if (downCell != null)
            if (downCell.GetDir() == BuildingTypeSO.Dir.Up) surroundingDirs[0] = true;
        if (leftCell != null)
            if (leftCell.GetDir() == BuildingTypeSO.Dir.Right) surroundingDirs[1] = true;
        if (upCell != null)
            if (upCell.GetDir() == BuildingTypeSO.Dir.Down) surroundingDirs[2] = true;
        if (rightCell != null)
            if (rightCell.GetDir() == BuildingTypeSO.Dir.Left) surroundingDirs[3] = true;

        List<string> names = new List<string>();
        switch (dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:        
                if (surroundingDirs[1]) names.Add("CurveRight");
                if (surroundingDirs[2]) names.Add("Straight");
                if (surroundingDirs[3]) names.Add("CurveLeft");
                break;
            case BuildingTypeSO.Dir.Up:
                if (surroundingDirs[0]) names.Add("Straight");
                if (surroundingDirs[1]) names.Add("CurveLeft");
                if (surroundingDirs[3]) names.Add("CurveRight");
                break;
            case BuildingTypeSO.Dir.Left:
                if (surroundingDirs[0]) names.Add("CurveLeft");
                if (surroundingDirs[2]) names.Add("CurveRight");
                if (surroundingDirs[3]) names.Add("Straight");
                break;
            case BuildingTypeSO.Dir.Right:
                if (surroundingDirs[0]) names.Add("CurveRight");
                if (surroundingDirs[1]) names.Add("Straight");
                if (surroundingDirs[2]) names.Add("CurveLeft");
                break;
        }
        if (names.Count == 0) names.Add("Straight");
        SetModelDir(names.ToArray());
    }
    public bool Occupied() => worldItem != null;

    private void MoveWorldItem(object sender, TimeTickSystem.TickEventArgs e)
    {
        int x, y;
        GridBuildingSystem.Instance.GetGrid().GetXY(transform.position, out x, out y);
        Vector2Int dirVector = GetDirectionVector(dir);
        Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
        float cellSize = GridBuildingSystem.Instance.GetGrid().GetCellSize();

        int currentX, currentY;
        int nextX, nextY;
        int previousX, previousY;

        currentX = x - rotationOffset.x;
        currentY = y - rotationOffset.y;
        nextX = currentX + dirVector.x;
        nextY = currentY + dirVector.y;
        previousX = currentX - dirVector.x;
        previousY = currentY - dirVector.y;

        if (Occupied())
        {
            // Move world item to next cell, so long as the next cell isn't occupied
            Building nextCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(nextX, nextY).GetBuilding();

            // Check if the cell is a container and if the cell isn't occupied
            if (nextCell != null)
            {
                Vector3 newPosition = GridBuildingSystem.Instance.GetGrid().GetWorldPosition(nextX, nextY) + (0.5f*new Vector3(cellSize, 3f, cellSize));

                if (nextCell.GetBuildingTypeSO().nameString == "Conveyor")
                {
                    if (!((Conveyor)nextCell).Occupied() && !worldItem.Moved)
                    {
                        ((Conveyor)nextCell).SetItem(worldItem);
                        worldItem.MoveTo(newPosition);
                        SetItem(null);
                    }
                }
                
                if (nextCell.GetBuildingTypeSO().nameString == "Large Building" && !worldItem.Moved)
                {
                    worldItem.MoveTo(newPosition);
                    SetItem(null);
                }
            }
        }

        // Check if occupation has ended
        if (!Occupied())
        {
            // Check previous cell in link to see if its next to an emmiter
            Building prevCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(previousX, previousY).GetBuilding();
            if (prevCell != null)
            {
                if (prevCell.GetBuildingTypeSO().nameString == "Small Building")
                {
                    WorldItem newItem = Instantiate(prefab, GridBuildingSystem.Instance.GetGrid().GetWorldPosition(previousX, previousY) + (0.5f * new Vector3(cellSize, 3f, cellSize)), Quaternion.identity);
                    newItem.MoveTo(GridBuildingSystem.Instance.GetGrid().GetWorldPosition(currentX, currentY) + (0.5f * new Vector3(cellSize, 3f, cellSize)));
                    SetItem(newItem);
                }
            }
        }
    }

    public void SetItem(WorldItem item)
    {
        worldItem = item;
    }

    public WorldItem GetItem() => worldItem;

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

    void OnDestroy()
    {
        TimeTickSystem.OnTick -= MoveWorldItem;
    }
}

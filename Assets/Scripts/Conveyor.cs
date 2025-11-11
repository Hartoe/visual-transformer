using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : Building
{
    //! For Testing
    [SerializeField] WorldItem prefab;

    WorldItem worldItem;

    void Start()
    {
        TimeTickSystem.OnTick += MoveWorldItem;

        //TODO: Check surroundings for other conveyorbelts that lead into this one and change the model
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

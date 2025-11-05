using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] List<BuildingTypeSO> buildingList;
    private BuildingTypeSO selectedBuilding;
    [SerializeField] InputActionReference lmb;
    private Grid<GridObject> grid;
    private BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;

    private void Awake()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 15f;
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, Vector3.zero, (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));

        selectedBuilding = buildingList[0];
    }

    public class GridObject
    {
        private Grid<GridObject> grid;
        private int x;
        private int y;
        private Building building;

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void SetBuilding(Building building)
        {
            this.building = building;
            grid.TriggerGridObjectChanged(x, y);
        }

        public Building GetBuilding()
        {
            return building;
        }

        public void ClearBuilding()
        {
            building = null;
            grid.TriggerGridObjectChanged(x, y);
        }

        public bool CanBuild() => building == null;

        public override string ToString()
        {
            return $"{x}, {y}\n{building}";
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x, y;
            grid.GetXY(Utilities.Input.MouseToWorldPosition(), out x, out y);

            List<Vector2Int> gridPositionList = selectedBuilding.GetGridPositionList(new Vector2Int(x, y), dir);
            bool canBuild = true;
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                {
                    canBuild = false;
                    break;
                }
            }
            if (canBuild)
            {
                Vector2Int rotationOffset = selectedBuilding.GetRotationOffset(dir);
                Vector3 buildingWorldPosition = grid.GetWorldPosition(x, y) +
                                                new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                Building building = Building.Create(buildingWorldPosition, new Vector2Int(x, y), dir, selectedBuilding);
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).SetBuilding(building);
                }
            }
            else
            {
                //TODO: Implement GUI popups
                // Utilities.GUI.CreateWorldTextPopup("Cannot Build Here!", Utilities.Input.MouseToWorldPosition());
                Debug.Log("Cannot Build Here");
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            GridObject gridObject = grid.GetGridObject(Utilities.Input.MouseToWorldPosition());
            Building building = gridObject.GetBuilding();
            if (building != null)
            {
                building.DestroySelf();
                List<Vector2Int> gridPositionList = building.GetGridPositionList();
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).ClearBuilding();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            dir = BuildingTypeSO.GetNextDir(dir);
            Debug.Log(dir);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedBuilding = buildingList[0];
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedBuilding = buildingList[1];
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedBuilding = buildingList[2];
    }
}

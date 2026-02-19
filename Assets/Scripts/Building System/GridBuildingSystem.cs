using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance;
    public event EventHandler<EventArgs> OnSelectedChanged;

    [Header("Grid Size")]
    [SerializeField] int gridWidth = 10;
    [SerializeField] int gridHeight = 10;
    [SerializeField] float cellSize = 15f;

    [Header("List of Buildings")]
    [SerializeField] List<BuildingTypeSO> buildingList;
    private BuildingTypeSO selectedBuilding;

    [Header("Button action")]
    [SerializeField] InputActionReference lmb;
    private Grid<GridObject> grid;
    private BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;
    private bool buildingActive = true;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, Vector3.zero, (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));

        selectedBuilding = buildingList[0];
    }

    public List<BuildingTypeSO> GetBuildingList() => buildingList;

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

        public Vector3 Center() => grid.GetWorldPosition(x,y) + (0.5f*new Vector3(grid.GetCellSize(), 3f, grid.GetCellSize()));

        public override string ToString()
        {
            return $"{x}, {y}\n{building}";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) SetBuildActive(!buildingActive);
        if (buildingActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                int x, y;
                grid.GetXY(Utilities.Input.MouseToWorldPosition(), out x, out y);

                List<Vector2Int> gridPositionList = selectedBuilding.GetGridPositionList(new Vector2Int(x, y), dir);
                bool canBuild = true;
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    GridObject go = grid.GetGridObject(gridPosition.x, gridPosition.y);
                    if (go == null || !go.CanBuild())
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
                    Utilities.GUI.CreateWorldTextPopup("Cannot Build Here!", localPosition: Utilities.Input.MouseToWorldPosition(), color: Color.red);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                GridObject gridObject = grid.GetGridObject(Utilities.Input.MouseToWorldPosition());
                if (gridObject != null)
                {
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
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                dir = BuildingTypeSO.GetNextDir(dir);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) && buildingList.Count > 0) SetBuildingType(buildingList[0]);   
            if (Input.GetKeyDown(KeyCode.Alpha2) && buildingList.Count > 1) SetBuildingType(buildingList[1]);
            if (Input.GetKeyDown(KeyCode.Alpha3) && buildingList.Count > 2) SetBuildingType(buildingList[2]);
            if (Input.GetKeyDown(KeyCode.Alpha4) && buildingList.Count > 3) SetBuildingType(buildingList[3]);   
            if (Input.GetKeyDown(KeyCode.Alpha5) && buildingList.Count > 4) SetBuildingType(buildingList[4]);
            if (Input.GetKeyDown(KeyCode.Alpha6) && buildingList.Count > 5) SetBuildingType(buildingList[5]);
            if (Input.GetKeyDown(KeyCode.Alpha7) && buildingList.Count > 6) SetBuildingType(buildingList[6]);
            if (Input.GetKeyDown(KeyCode.Alpha8) && buildingList.Count > 7) SetBuildingType(buildingList[7]);
            if (Input.GetKeyDown(KeyCode.Alpha9) && buildingList.Count > 8) SetBuildingType(buildingList[8]);
            if (Input.GetKeyDown(KeyCode.Alpha0) && buildingList.Count > 9) SetBuildingType(buildingList[9]);
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                int x, y;
                grid.GetXY(Utilities.Input.MouseToWorldPosition(), out x, out y);

                GridObject selectedCell = grid.GetGridObject(x, y);
                if (selectedCell != null)
                {
                    Building building = selectedCell.GetBuilding();
                    if (building != null)
                    {
                        // Show building info screen
                        building.ShowInfoPanel();
                    }
                }
            }
        }
    }

    public void SetBuildingType(BuildingTypeSO building)
    {
        selectedBuilding = building;
        OnSelectedChanged.Invoke(this, EventArgs.Empty);
    }

    public void SetBuildActive(bool value)
    {
        buildingActive = value;
    }

    public bool GetBuildActive() => buildingActive;

    public Vector3 GetMouseWorldSnappedPosition()
    {
        Vector3 rawPosition = Utilities.Input.MouseToWorldPosition();
        float x = Mathf.FloorToInt(rawPosition.x / grid.GetCellSize()) * grid.GetCellSize();
        float z = Mathf.FloorToInt(rawPosition.z / grid.GetCellSize()) * grid.GetCellSize();

        return new Vector3(x, 0, z);
    }

    public Quaternion GetBuildingRotation()
    {
        return Quaternion.Euler(0, selectedBuilding.GetRotationAngle(dir), 0);
    }

    public Vector3 GetBuildingPositionOffset()
    {
        Vector2Int offset = selectedBuilding.GetRotationOffset(dir);
        return new Vector3(offset.x, 0, offset.y) * grid.GetCellSize();
    }

    public BuildingTypeSO GetBuildingTypeSO() => selectedBuilding;
    public Grid<GridObject> GetGrid() => grid;
    public void ResetGrid()
    {
        (int width, int height) = grid.Size;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridObject gridObject = grid.GetGridObject(x, y);
                if (gridObject != null)
                {
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
            }
        }
    }
}

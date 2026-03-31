using System.Collections.Generic;
using UnityEngine;

public abstract class AFactory : Building
{
        [SerializeField] GameObject SmokeParticles;
        [SerializeField] BrokenMenu brokenMenu;
        public List<(int, int)> OutputCells { get; protected set; }
        public List<(int, int)> InputCells { get; protected set; }
        public List<(WorldItem, (int, int))> NewItems { get; protected set; }
        protected int cellX, cellY;
        protected bool broken = false;
        protected GameObject smokeInstance;
        protected Vector3 Center => GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center();

        protected void Start()
        {
                OutputCells = new List<(int, int)>();
                InputCells = new List<(int, int)>();
                NewItems = new List<(WorldItem, (int, int))>();
        
                int x, y;
                GridBuildingSystem.Instance.GetGrid().GetXY(transform.position, out x, out y);
                Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
                cellX = x - rotationOffset.x;
                cellY = y - rotationOffset.y;
        
                TimeTickSystem.OnTick += Action;
        
                FillCellLists();

                // Check each output cell for a conveyor and update its model
                foreach (var coords in OutputCells)
                {
                        GridBuildingSystem.GridObject cell = GridBuildingSystem.Instance.GetGrid().GetGridObject(coords.Item1, coords.Item2);
                        if (cell == null) continue;
                        Building building = cell.GetBuilding();
                        if (building == null) continue;
                        if (building.GetBuildingTypeSO().nameString == "Conveyor")
                        {
                                ((Conveyor)building).UpdateConveyorModel();
                        }
                }
        }

        public abstract void AddFromInput(WorldItem item, (int, int) cell);
        public abstract WorldItem RemoveFromOutput((int, int) cell);

        protected abstract void Action(object sender, TimeTickSystem.TickEventArgs e);
        protected abstract void FillCellLists();
        protected abstract void Reset();

        protected void OnDestroy()
        {
                TimeTickSystem.OnTick -= Action;
                if (smokeInstance != null) Destroy(smokeInstance);
        }

        public virtual bool Occupied((int, int) cell)
        {
                return broken;
        }

        protected void Break(string message)
        {
                broken = true;
                Reset();
                if (smokeInstance != null) Destroy(smokeInstance);
                smokeInstance = Instantiate(SmokeParticles, Center, Quaternion.Euler(-90, 0, 0));
                brokenMenu.gameObject.SetActive(true);
                brokenMenu.SetMessage(message);
        }

        public void Fix()
        {
                broken = false;
                Destroy(smokeInstance);
                brokenMenu.gameObject.SetActive(false);
                Reset();
        }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AFactory : Building
{
        public List<(int, int)> OutputCells { get; protected set; }
        public List<(int, int)> InputCells { get; protected set; }
        protected int cellX, cellY;

        protected void Start()
        {
                OutputCells = new List<(int, int)>();
                InputCells = new List<(int, int)>();
        
                int x, y;
                GridBuildingSystem.Instance.GetGrid().GetXY(transform.position, out x, out y);
                Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
                cellX = x - rotationOffset.x;
                cellY = y - rotationOffset.y;
        
                TimeTickSystem.OnTick += Action;
        
                FillCellLists();
        }

        public abstract void AddFromInput(WorldItem item, (int, int) cell);
        public abstract WorldItem RemoveFromOutput((int, int) cell);

        protected abstract void Action(object sender, TimeTickSystem.TickEventArgs e);
        protected abstract void FillCellLists();

        protected void OnDestroy()
        {
            TimeTickSystem.OnTick -= Action;
        }
}
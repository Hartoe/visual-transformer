using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;

public abstract class AFactory : Building
{
        [SerializeField] float wiggleScale = 0.05f;
        [SerializeField] bool unbreakable = false;
        [SerializeField] GameObject SmokeParticles;
        [SerializeField] BrokenMenu brokenMenu;
        public class FactoryArgs
        {
                public string nameString;
                public Matrix state;
                public FactoryArgs(string nameString, Matrix state)
                {
                        this.nameString = nameString;
                        this.state = state;
                }
        }
        public static event EventHandler<FactoryArgs> OnActionComplete;
        public static void Invoke(object sender, string name, Matrix state)
        {
                OnActionComplete.Invoke(sender, new FactoryArgs(name, state));
        }
        public List<(int, int)> OutputCells { get; protected set; }
        public List<(int, int)> InputCells { get; protected set; }
        public List<(WorldItem, (int, int))> NewItems { get; protected set; }
        protected int cellX, cellY;
        protected bool broken = false;
        protected GameObject smokeInstance;
        protected Vector3 Center => GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center();
        protected Coroutine coroutineOnAction;

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
                return broken || !InputCells.Contains(cell);
        }

        protected void Break(string message)
        {
                if (unbreakable) return;
                broken = true;
                Reset();
                if (smokeInstance != null) Destroy(smokeInstance);
                smokeInstance = Instantiate(SmokeParticles, Center, Quaternion.Euler(-90, 0, 0));
                brokenMenu.gameObject.SetActive(true);
                brokenMenu.SetMessage(message);
                OnActionComplete.Invoke(this, new FactoryArgs(buildingTypeSO.nameString + " Break", Matrix.Identity(1)));
        }

        public void Fix()
        {
                broken = false;
                Destroy(smokeInstance);
                brokenMenu.gameObject.SetActive(false);
                Reset();
                OnActionComplete.Invoke(this, new FactoryArgs(buildingTypeSO.nameString + " Fix", Matrix.Identity(1)));
        }

        protected void Invoke(string nameString, Matrix state)
        {
                if (this == null || nameString == null || state == null || OnActionComplete == null) return;
                OnActionComplete.Invoke(this, new FactoryArgs(nameString, state));
        }

        protected IEnumerator AnimateOnAction()
        {
                float time = 0;
                while (true)
                {
                        time += Time.deltaTime;
                        time = time % (2 * Mathf.PI);

                        float xScale = Mathf.Sin(time);
                        float yScale = Mathf.Cos(time);
                        float zScale = Mathf.Sin(time - (Mathf.PI/2));
                        Vector3 wiggle = new Vector3(xScale, yScale, zScale);
                        gameObject.transform.localScale = Vector3.one + (wiggleScale * wiggle);

                        yield return null;
                }
        }

        protected void StartAnimation()
        {
                if (coroutineOnAction == null) coroutineOnAction = StartCoroutine(AnimateOnAction());
        }
        protected void StopAnimation()
        {
                if (coroutineOnAction != null)
                {
                        StopCoroutine(coroutineOnAction);
                        coroutineOnAction = null;
                }
                gameObject.transform.localScale = Vector3.one;
        }
}
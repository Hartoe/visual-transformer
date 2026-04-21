using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public List<Conveyor> conveyors = new List<Conveyor>();
    public List<AFactory> factories = new List<AFactory>();

    void Start()
    {
        TimeTickSystem.OnTick += UpdateOnTick;
    }

    private void UpdateOnTick(object sender, TimeTickSystem.TickEventArgs e)
    {
        Snapshot();
        Simulate();
        Apply();
    }

    void Snapshot()
    {
        foreach (var belt in conveyors)
        {
            belt.occupiedAtStart = belt.currentItem != null;
            belt.reserved = false;
            belt.nextItem = null;
        }

        foreach (var factory in factories)
        {
            if (factory != null) factory.NewItems.Clear();
        }
    }

    void Simulate()
    {
        foreach (var belt in conveyors)
        {
            TryMove(belt);
        }

        foreach (var factory in factories)
        {
            TryOutput(factory);
        }
    }

    private void TryOutput(AFactory factory)
    {
        foreach (var cell in factory.OutputCells)
        {
            GridBuildingSystem.GridObject nextCell = GridBuildingSystem.Instance.GetGrid().GetGridObject(cell.Item1, cell.Item2);
            if (nextCell != null)
            {
                Building building = nextCell.GetBuilding();
                if (building != null)
                {
                    if (building is Conveyor)
                    {
                        Conveyor nextBelt = (Conveyor)building;
                        if (!nextBelt.occupiedAtStart && !nextBelt.reserved)
                        {
                            WorldItem nextItem = factory.RemoveFromOutput(cell);
                            if (nextItem != null)
                            {
                                nextBelt.reserved = true;
                                nextBelt.nextItem = nextItem;
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    private void TryMove(Conveyor belt)
    {
        if (!belt.occupiedAtStart)
            return;
        
        GridBuildingSystem.GridObject nextCell = belt.NextCell;
        if (nextCell != null)
        {
            Building building = nextCell.GetBuilding();
            if (building != null)
            {
                if (building is Conveyor)
                {
                    Conveyor nextBelt = (Conveyor)building;
                    if (!nextBelt.occupiedAtStart && !nextBelt.reserved)
                    {
                        nextBelt.reserved = true;
                        nextBelt.nextItem = belt.currentItem;
                        return;
                    }
                }
                else if (building is AFactory)
                {
                    AFactory nextFactory = (AFactory)building;
                    if (!nextFactory.Occupied(belt.CellPosition()))
                    {
                        nextFactory.NewItems.Add((belt.currentItem, belt.CellPosition()));
                        return;
                    } 
                }
            }
        }

        belt.nextItem = belt.currentItem;
    }

    void Apply()
    {
        foreach (var belt in conveyors)
        {
            belt.SetItem(belt.nextItem);
        }

        foreach (var factory in factories)
        {
            foreach (var kvp in factory.NewItems)
            {
                factory.AddFromInput(kvp.Item1, kvp.Item2);
            }
        }
    }
}

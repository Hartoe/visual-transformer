using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class OutputNode : AFactory
{
    private Matrix test; //TODO: Make way to set this matrix
    private List<Matrix> inputs = new List<Matrix>();

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        inputs.Add(item.state);
        Destroy(item.gameObject);
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        throw new System.NotImplementedException();
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix check = inputs.First();
            inputs.RemoveAt(0);
            Debug.Log(check);
            if (CheckSimilar(check, test)) Debug.Log($"Matrices match!");
            else Debug.Log("Matrices don't match!");
        }
    }

    private bool CheckSimilar(Matrix A, Matrix B) => false;

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Left:
                InputCells.Add((cellX + 1, cellY));
                break;
            case BuildingTypeSO.Dir.Up:
                InputCells.Add((cellX, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class OutputNode : AFactory
{
    [SerializeField] double epsilon = 0.0001;
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
            UpdateInfoPanel();
            Matrix check = inputs.First();
            inputs.RemoveAt(0);
            Debug.Log(check);
            if (CheckSimilar(check, test)) Debug.Log($"Matrices match!");
            else Debug.Log("Matrices don't match!");
        }
    }

    private bool CheckSimilar(Matrix A, Matrix B)
    {
        if (A.Shape != B.Shape) return false;

        for (int i = 0; i < A.Rows; i++)
        {
            for (int j = 0; j < A.Columns; j++)
            {
                if (Math.Abs(A[i,j] - B[i,j]) > epsilon)
                    return false;
            }
        }
        return true;
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            OutputInfoPanel panel = infoPanelInstance.GetComponent<OutputInfoPanel>();
            if (inputs.Count > 0)
            {
                Matrix current = inputs.First();
                panel.SetText(current, CheckSimilar(current, test));
            }
            else
                panel.SetText(null);
        }
    }

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class MultiplicationFactory : AFactory
{
    [SerializeField] Intent itemPrefab;

    private List<Intent> outputs = new List<Intent>();
    private Matrix? A, B;

    private bool matrixASet, matrixBSet;
    
    new void Start()
    {
        TimeTickSystem.OnTick += UpdateInfoPanel;
        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        if (cell == InputCells[0]) {
            // If A is null, set A and put the factory on occupied
            if (!matrixASet)
            {
                A = item.state;
                matrixASet = true;
            }
        } else if (cell == InputCells[1]) {
            // If B is null, set B and put the factory on occupied
            if (!matrixBSet)
            {
                B = item.state;
                matrixBSet = true;
            }
        }
        Destroy(item.gameObject);
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        // Check if outputs list is empty, return null
        if (outputs.Count <= 0) return null;

        // if not pop first item
        WorldItem item = outputs.First();
        outputs.RemoveAt(0);

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (A != null && B != null)
        {
            try
            {
                Matrix C = ((Matrix)A) * ((Matrix)B);
                Intent output = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
                output.state = C;
                A = null;
                B = null;
                outputs.Add(output);
            }
            catch
            {
                //TODO: handle wrong dimension passed
                Debug.Log("Wrong matrix dimensions given!");
            }
        }
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX + 1, cellY));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX, cellY - 1));
                break;
        }
    }

    public override bool Occupied((int, int) cell)
    {
        if (cell == InputCells[0]) return matrixASet;
        if (cell == InputCells[1]) return matrixBSet;
        return false;
    }

    private void UpdateInfoPanel(object sender, TimeTickSystem.TickEventArgs e)
    {
        UpdateInfoPanel();
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            MathInfoPanel panel = infoPanelInstance.GetComponent<MathInfoPanel>();
            if (A != null)
                panel.SetMatrixA((Matrix)A);
            else
                panel.SetMatrixA();
            if (B != null)
                panel.SetMatrixB((Matrix)B);
            else
                panel.SetMatrixB();

            if (panel.AddListener)
            {
                panel.Button.onClick.AddListener(ClearMatrices);
                panel.AddListener = false;
            }
        }
    }

    private void ClearMatrices()
    {
        A = null;
        B = null;
        matrixASet = false;
        matrixBSet = false;
        UpdateInfoPanel();
    }

    new void OnDestroy()
    {
        foreach (Intent item in outputs)
            Destroy(item.gameObject);
        TimeTickSystem.OnTick -= UpdateInfoPanel;
        base.OnDestroy();
    }
}

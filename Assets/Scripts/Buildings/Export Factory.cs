using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Utilities.ML;
using static Utilities.JSON;

public class ExportFactory : PassThroughFactory
{
    public static UnityEvent OnLevelComplete = new UnityEvent();
    [SerializeField] double epsilon = 0.0001;
    [SerializeField] string JSONFilePath;
    private Matrix expectedMatrix;

    new void Start()
    {
        if (JSONFilePath != "") expectedMatrix = JSONToMatrix(LoadJSONFile(JSONFilePath));
        else expectedMatrix = new Matrix();
        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        inputs.Add(item.state);
        if (coroutineOnAction == null) StartAnimation();
        Destroy(item.gameObject);
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            UpdateInfoPanel();
            Matrix check = inputs.First();
            inputs.RemoveAt(0);
            if (check.Similar(expectedMatrix)) OnLevelComplete.Invoke();
            else Break("The input does not match the expected output!");
            StopAnimation();
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            OutputInfoPanel panel = infoPanelInstance.GetComponent<OutputInfoPanel>();
            if (inputs.Count > 0)
            {
                Matrix current = inputs.First();
                panel.SetText(current, current.Similar(expectedMatrix));
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

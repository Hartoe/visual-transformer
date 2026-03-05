using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class AttentionFactory : AFactory
{
    [SerializeField] int rows;
    [SerializeField] int columns;
    [SerializeField] Intent itemPrefab;
    [SerializeField] string queryJSONPath;
    [SerializeField] string keyJSONPath;
    [SerializeField] string valueJSONPath;
    private List<Intent> outputs = new List<Intent>();
    private Matrix? Q, K, V;
    private Attention attention;
    private bool qSet, kSet, vSet;
    private string pathName;

    new void Start()
    {
        attention = new Attention(rows, columns);
        qSet = false;
        kSet = false;
        vSet = false;

        if (!string.IsNullOrEmpty(queryJSONPath))
        {
            JSON.LayerJSON queryJSON = JSON.JSONToLayer(JSON.LoadJSONFile(queryJSONPath));
            attention.queryLayer.SetWeights(queryJSON.weights.ToMatrix());
            attention.queryLayer.SetBiases(queryJSON.biases.ToMatrix());
        }
        if (!string.IsNullOrEmpty(keyJSONPath))
        {
            JSON.LayerJSON keyJSON = JSON.JSONToLayer(JSON.LoadJSONFile(keyJSONPath));
            attention.keyLayer.SetWeights(keyJSON.weights.ToMatrix());
            attention.keyLayer.SetBiases(keyJSON.biases.ToMatrix());
        }
        if (!string.IsNullOrEmpty(valueJSONPath))
        {
            JSON.LayerJSON valueJSON = JSON.JSONToLayer(JSON.LoadJSONFile(valueJSONPath));
            attention.valueLayer.SetWeights(valueJSON.weights.ToMatrix());
            attention.valueLayer.SetBiases(valueJSON.biases.ToMatrix());
        }

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        if (cell == InputCells[0]) {
            // If A is null, set A and put the factory on occupied
            if (!vSet)
            {
                V = item.state;
                vSet = true;
            }
        } else if (cell == InputCells[1]) {
            // If B is null, set B and put the factory on occupied
            if (!kSet)
            {
                K = item.state;
                kSet = true;
            }
        } else if (cell == InputCells[2]) {
            // If B is null, set B and put the factory on occupied
            if (!qSet)
            {
                Q = item.state;
                qSet = true;
            }
        }
        item.MoveTo(Center);
        item.DestroyOnArrival();
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
        if (Q != null && V != null && K != null)
        {
            attention.Queries = (Matrix)Q;
            attention.Values = (Matrix)V;
            attention.Keys = (Matrix)K;

            try
            {
                Matrix outputState = attention.CalculateOutputs((Matrix)Q);
                Intent output = Instantiate(itemPrefab, Center, Quaternion.identity);
                output.state = outputState;
                K = null;
                V = null;
                Q = null;
                outputs.Add(output);
            } catch
            {
                Break("Incompatible dimensions between matrices!");
            }
        }
    }

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX + 2, cellY + 1));
                InputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY + 1));
                InputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX + 1, cellY + 2));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX + 1, cellY - 1));
                InputCells.Add((cellX + 2, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY + 1));
                InputCells.Add((cellX - 1, cellY + 2));
                InputCells.Add((cellX - 1, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            AttentionInfoPanel panel = infoPanelInstance.GetComponent<AttentionInfoPanel>();
            if (rows != int.Parse(panel.rowsInput.text))
                panel.rowsInput.text = rows.ToString();
            if (columns != int.Parse(panel.columnsInput.text))
                panel.columnsInput.text = columns.ToString();
            if (panel.weightsDropdown != null)
            {
                if (!string.IsNullOrEmpty(pathName) && pathName != panel.weightsDropdown.options[panel.weightsDropdown.value].text)
                {
                    var optionsList = panel.weightsDropdown.options.Select(option => option.text).ToList();
                    panel.weightsDropdown.value = optionsList.IndexOf(pathName);
                }
                panel.weightsDropdown.onValueChanged.AddListener(ReloadWeightsAndBiases);
            }

            panel.rowsInput.onValueChanged.AddListener(ChangeRowValue);
            panel.columnsInput.onValueChanged.AddListener(ChangeColumnValue);
        }
    }

    private void ReloadWeightsAndBiases(int arg0)
    {
        AttentionInfoPanel panel = infoPanelInstance.GetComponent<AttentionInfoPanel>();
        pathName = panel.weightsDropdown.options[arg0].text;
        JSON.LayerJSON queryJSON = JSON.JSONToLayer(JSON.LoadJSONFile($"/lvl_{panel.level}/{pathName}_query_weights.json"));
        JSON.LayerJSON keyJSON = JSON.JSONToLayer(JSON.LoadJSONFile($"/lvl_{panel.level}/{pathName}_key_weights.json"));
        JSON.LayerJSON valueJSON = JSON.JSONToLayer(JSON.LoadJSONFile($"/lvl_{panel.level}/{pathName}_value_weights.json"));

        attention.queryLayer.SetWeights(queryJSON.weights.ToMatrix());
        attention.queryLayer.SetBiases(queryJSON.biases.ToMatrix());
        attention.keyLayer.SetWeights(keyJSON.weights.ToMatrix());
        attention.keyLayer.SetBiases(keyJSON.biases.ToMatrix());
        attention.valueLayer.SetWeights(valueJSON.weights.ToMatrix());
        attention.valueLayer.SetBiases(valueJSON.biases.ToMatrix());
    }

    private void ChangeRowValue(string value)
    {
        rows = int.Parse(value);
        if (rows <= 0) rows = 1;

        attention = new Attention(rows, columns);
    }
    private void ChangeColumnValue(string value)
    {
        columns = int.Parse(value);
        if (columns <= 0) columns = 1;

        attention = new Attention(rows, columns);
    }

    public override bool Occupied((int, int) cell)
    {
        if (cell == InputCells[0]) return vSet || broken;
        if (cell == InputCells[1]) return kSet || broken;
        if (cell == InputCells[2]) return qSet || broken;
        return broken;
    }

    protected override void Reset()
    {
        outputs = new List<Intent>();
        Q = null;
        V = null;
        K = null;
        qSet = false;
        vSet = false;
        kSet = false;
    }
}

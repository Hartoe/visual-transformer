using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class NetworkFactory : AFactory
{
    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [SerializeField] Intent itemPrefab;
    [SerializeField] int inputRows = 7;
    [SerializeField] int inputColumns = 7;
    [SerializeField] int hiddenRows = 7;
    [SerializeField] int hiddenColumns = 20;
    [SerializeField] int outputRows = 7;
    [SerializeField] int outputColumns = 7;

    private FullyConnectedNN network;
    private List<WorldItem> outputs = new List<WorldItem>();
    private List<Matrix> inputs = new List<Matrix>();

    new void Start()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetCostFunction(Cost.GetCostFromType(costType));

        //TODO: Find a way to pass pre-trained weights and biases

        base.Start();
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        // Check if item is a RESOURCE
        if (item is Intent)
        {
            // Save the matrix of the item
            inputs.Add(item.state);
        }

        //TODO: Handle wrong input with smoke effect and popup

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
        if (inputs.Count > 0)
        {
            try
            {
                Matrix input = inputs.First();
                inputs.RemoveAt(0);
                Matrix output = network.CalculateOutputs(input);
                WorldItem newItem = Instantiate(itemPrefab, GridBuildingSystem.Instance.GetGrid().GetGridObject(cellX, cellY).Center(), Quaternion.identity);
                newItem.state = output;
                outputs.Add(newItem);
            }
            catch
            {
                //TODO: Handle wrong dimensions of input
                Debug.Log("Wrong dimensions of input");
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
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 1, cellY));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX, cellY - 1));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX - 1, cellY));
                break;
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            NetworkInfoPanel panel = infoPanelInstance.GetComponent<NetworkInfoPanel>();
            if ((int)costType != panel.costDropdown.value)
                panel.costDropdown.value = (int)costType;
            if ((int)activationType != panel.activationDropdown.value)
                panel.activationDropdown.value = (int)activationType;
            if (inputRows != int.Parse(panel.inputRows.text))
                panel.inputRows.text = inputRows.ToString();
            if (inputColumns != int.Parse(panel.inputColumns.text))
                panel.inputColumns.text = inputColumns.ToString();
            if (hiddenRows != int.Parse(panel.hiddenRows.text))
                panel.hiddenRows.text = hiddenRows.ToString();
            if (hiddenColumns != int.Parse(panel.hiddenColumns.text))
                panel.hiddenColumns.text = hiddenColumns.ToString();
            if (outputRows != int.Parse(panel.outputRows.text))
                panel.outputRows.text = outputRows.ToString();
            if (outputColumns != int.Parse(panel.outputColumns.text))
                panel.outputColumns.text = outputColumns.ToString();

            panel.costDropdown.onValueChanged.AddListener(SetCostFunction);
            panel.activationDropdown.onValueChanged.AddListener(SetActivationFunction);
            panel.inputRows.onValueChanged.AddListener(SetInputRows);
            panel.inputColumns.onValueChanged.AddListener(SetInputColumns);
            panel.hiddenRows.onValueChanged.AddListener(SetHiddenRows);
            panel.hiddenColumns.onValueChanged.AddListener(SetHiddenColumns);
            panel.outputRows.onValueChanged.AddListener(SetOutputRows);
            panel.outputColumns.onValueChanged.AddListener(SetOutputColumns);
        }
    }

    private void SetOutputColumns(string arg0)
    {
        outputColumns = int.Parse(arg0);
        if (outputColumns <= 0) outputColumns = 1;
        ReloadNetwork();
    }

    private void SetOutputRows(string arg0)
    {
        outputRows = int.Parse(arg0);
        if (outputRows <= 0) outputRows = 1;
        ReloadNetwork();
    }

    private void SetHiddenColumns(string arg0)
    {
        hiddenColumns = int.Parse(arg0);
        if (hiddenColumns <= 0) hiddenColumns = 1;
        ReloadNetwork();
    }

    private void SetHiddenRows(string arg0)
    {
        hiddenRows = int.Parse(arg0);
        if (hiddenRows <= 0) hiddenRows = 1;
        ReloadNetwork();
    }

    private void SetInputColumns(string arg0)
    {
        inputColumns = int.Parse(arg0);
        if (inputColumns <= 0) inputColumns = 1;
        ReloadNetwork();
    }

    private void SetInputRows(string arg0)
    {
        inputRows = int.Parse(arg0);
        if (inputRows <= 0) inputRows = 1;
        ReloadNetwork();
    }

    private void SetActivationFunction(int arg0)
    {
        activationType = (Activation.ActivationType)arg0;
        ReloadNetwork();
    }

    private void SetCostFunction(int arg0)
    {
        costType = (Cost.CostType)arg0;
        ReloadNetwork();
    }
 
    private void ReloadNetwork()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        //TODO: Hotload weights and biases
    }

    new void OnDestroy()
    {
        foreach (WorldItem item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

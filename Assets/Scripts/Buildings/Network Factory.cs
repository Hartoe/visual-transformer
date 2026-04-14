using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class NetworkFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;
    [SerializeField] string weightsJSONPath;
    [Header("Network Variables")]
    [SerializeField] Cost.CostType costType;
    [SerializeField] Activation.ActivationType activationType;
    [Header("Input Layer")]
    [SerializeField] int inputRows = 7;
    [SerializeField] int inputColumns = 7;
    [Header("Hidden Layer")]
    [SerializeField] int hiddenRows = 7;
    [SerializeField] int hiddenColumns = 20;
    [Header("Output Layer")]
    [SerializeField] int outputRows = 7;
    [SerializeField] int outputColumns = 7;

    private FullyConnectedNN network;

    new void Start()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetCostFunction(Cost.GetCostFromType(costType));

        if (weightsJSONPath != "")
        {
            string weightsJSON = JSON.LoadJSONFile(weightsJSONPath);
            JSON.NetworkJSON networkJSON = JSON.JSONToNetwork(weightsJSON);
            network.SetWeightsAndBiases(networkJSON);
        }

        base.Start();
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
                WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
                newItem.state = output;
                outputs.Add(newItem);
                Invoke(buildingTypeSO.nameString, newItem.state);
            }
            catch
            {
                Break("Incompatible matrix dimensions for the network!");
            }

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
            if (weightsJSONPath != JSON.GetJSONPath(panel.weightsDropdown.options[panel.weightsDropdown.value].text, panel.level))
            {
                var optionsList = panel.weightsDropdown.options.Select(option => option.text).ToList();
                panel.weightsDropdown.value = optionsList.IndexOf(JSON.GetFileName(weightsJSONPath));
            }

            panel.costDropdown.onValueChanged.AddListener(SetCostFunction);
            panel.activationDropdown.onValueChanged.AddListener(SetActivationFunction);
            panel.inputRows.onValueChanged.AddListener(SetInputRows);
            panel.inputColumns.onValueChanged.AddListener(SetInputColumns);
            panel.hiddenRows.onValueChanged.AddListener(SetHiddenRows);
            panel.hiddenColumns.onValueChanged.AddListener(SetHiddenColumns);
            panel.outputRows.onValueChanged.AddListener(SetOutputRows);
            panel.outputColumns.onValueChanged.AddListener(SetOutputColumns);
            panel.weightsDropdown.onValueChanged.AddListener(SetWeightsAndBiases);
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

    private void SetWeightsAndBiases(int arg0)
    {
        NetworkInfoPanel panel = infoPanelInstance.GetComponent<NetworkInfoPanel>();
        weightsJSONPath = JSON.GetJSONPath(panel.weightsDropdown.options[arg0].text, panel.level);
        ReloadNetwork();
    }
 
    private void ReloadNetwork()
    {
        network = new FullyConnectedNN((inputRows, inputColumns), (hiddenRows, hiddenColumns), (outputRows, outputColumns));
        network.SetCostFunction(Cost.GetCostFromType(costType));
        network.SetActivationFunction(Activation.GetActivationFromType(activationType));
        network.SetWeightsAndBiases(JSON.JSONToNetwork(JSON.LoadJSONFile(weightsJSONPath)));
    }
}

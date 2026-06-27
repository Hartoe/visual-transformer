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
            if ((int)activationType != panel.activationDropdown.value)
                panel.activationDropdown.value = (int)activationType;
            if (weightsJSONPath != JSON.GetJSONPath(panel.weightsDropdown.options[panel.weightsDropdown.value].text, panel.level))
            {
                var optionsList = panel.weightsDropdown.options.Select(option => option.text).ToList();
                panel.weightsDropdown.value = optionsList.IndexOf(JSON.GetFileName(weightsJSONPath));
            }

            panel.activationDropdown.onValueChanged.AddListener(SetActivationFunction);
            panel.weightsDropdown.onValueChanged.AddListener(SetWeightsAndBiases);
        }
    }

    private void SetActivationFunction(int arg0)
    {
        activationType = (Activation.ActivationType)arg0;
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

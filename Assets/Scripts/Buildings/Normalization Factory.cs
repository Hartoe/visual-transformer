using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class NormalizationFactory : PassThroughFactory
{
    [SerializeField] int rows;
    [SerializeField] int cols;
    [SerializeField] Intent itemPrefab;
    private LayerNorm layerNorm;

    new void Start()
    {
        layerNorm = new LayerNorm(rows, cols);
        base.Start();
    }


    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = layerNorm.CalculateOutputs(input);
            WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
            Invoke(buildingTypeSO.nameString, newItem.state);
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            NormalizationInfoPanel panel = infoPanelInstance.GetComponent<NormalizationInfoPanel>();
        }
    }
}

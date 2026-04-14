using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PositionalFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;
    private PositionalEmbedding positionalEmbedding = new PositionalEmbedding();

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = positionalEmbedding.CalculateOutputs(input);
            WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
            Invoke(buildingTypeSO.nameString, newItem.state);
        }
    }
}

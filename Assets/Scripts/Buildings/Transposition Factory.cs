using System.Linq;
using UnityEngine;
using Utilities.ML;

public class TranspositionFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;
    
    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = Matrix.T(input);
            Intent newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
        }
    }
}

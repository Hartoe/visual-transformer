using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class PickLastFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = new Matrix(1, input.Columns);
            for (int i = 0; i < output.Columns; i ++)
                output[0,i] = input[input.Rows-1,i];
            Intent newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
            Invoke(buildingTypeSO.nameString, newItem.state);
        }
    }
}

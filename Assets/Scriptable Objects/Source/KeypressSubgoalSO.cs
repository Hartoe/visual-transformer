using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;

[CreateAssetMenu()]
public class KeypressSubgoalSO : SubgoalSO
{
    [SerializeField] string keyName;

    private Matrix singleMatrix = Matrix.Identity(1);

    public override bool CheckComplete(string nameString, Matrix state)
    {
        return nameString == keyName && state.Similar(singleMatrix);
    }
}

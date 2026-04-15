using UnityEngine;
using Utilities.ML;

[CreateAssetMenu()]
public class BuildGoalSO : SubgoalSO
{
    [SerializeField] string buildingName;

    private Matrix singleMatrix = Matrix.Identity(1);

    public override bool CheckComplete(string nameString, Matrix state)
    {
        return buildingName == nameString && state.Similar(singleMatrix);
    }
}

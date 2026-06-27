using UnityEngine;
using Utilities;
using Utilities.ML;

[CreateAssetMenu()]
public class SubgoalSO : ScriptableObject
{
    public string Title;

    [SerializeField] string factoryName;

    [SerializeField] string matrixPath;

    private Matrix matrix;

    public virtual bool CheckComplete(string nameString, Matrix state)
    {
        // Load in the subgoal matrix from the path string
        string matrixJSON = JSON.LoadJSONFile(matrixPath);
        matrix = JSON.JSONToMatrix(matrixJSON);

        return nameString == factoryName && state.Similar(matrix);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ML;

[CreateAssetMenu()]
public class MultigoalSO : SubgoalSO
{
    [SerializeField] List<SubgoalSO> subgoals;
    private List<SubgoalSO> completed = new List<SubgoalSO>();

    public override bool CheckComplete(string nameString, Matrix state)
    {
        foreach (SubgoalSO goal in subgoals)
        {
            if (goal.CheckComplete(nameString, state) && !completed.Contains(goal))
            {
                completed.Add(goal);
                break;
            }
        }

        if (completed.Count == subgoals.Count)
        {
            completed.Clear();
            return true;
        }

        return false;
    }
}

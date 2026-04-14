using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SubgoalManager : MonoBehaviour
{
    public static SubgoalManager Instance;

    public UnityEvent<int> OnGoalComplete = new UnityEvent<int>();
    public SubgoalSO CurrentSubgoal { get { return currentSubgoal; } }

    [SerializeField] List<SubgoalSO> subgoals;
    private SubgoalSO currentSubgoal;
    private int currentIndex = 0;

    void Awake()
    {
        if (Instance != null)
            Destroy(this);

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        AFactory.OnActionComplete += CheckSubgoalComplete;
        currentSubgoal = subgoals[currentIndex];
    }

    public SubgoalSO GetGoal(int index)
    {
        return subgoals[index];
    }

    private void CheckSubgoalComplete(object sender, AFactory.FactoryArgs e)
    {
        if (currentIndex >= subgoals.Count)
            return;

        currentSubgoal = subgoals[currentIndex];

        // Check if the created state is equal to the next step in the subgoal list
        if (currentSubgoal.CheckComplete(e.nameString, e.state))
        {
            // If correct, check off the subgoal and increase current goal index
            OnGoalComplete.Invoke(currentIndex);
            currentIndex++;
        }
    }

    void OnDestroy()
    {
        AFactory.OnActionComplete -= CheckSubgoalComplete;
    }

    public void Reset()
    {
        currentIndex = 0;
        currentSubgoal = subgoals[currentIndex];
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MidgameGoalManager : MonoBehaviour
{
    [Serializable]
    public class MidgameCutsceneObject
    {
        public int GoalIndex;
        public List<CutsceneStep> Steps;
    }

    [SerializeField] SubgoalManager subgoalManager;
    [SerializeField] DialogueHandler dialogueHandler;
    [SerializeField] List<MidgameCutsceneObject> cutscenes;

    // Start is called before the first frame update
    void Start()
    {
        subgoalManager.OnGoalComplete.AddListener(StartNewCutscene);
    }

    private void StartNewCutscene(int index)
    {
        // Check if this goal has a cutscene
        MidgameCutsceneObject currentCutscene = cutscenes.Find(x => x.GoalIndex == index);

        if (currentCutscene == null) return;

        // Instantiate the MidgameCutscene object
        MidgameCutscene currentObject = new GameObject().AddComponent<MidgameCutscene>();

        // Add the given cutscene steps to the object
        currentObject.steps = currentCutscene.Steps;
        currentObject.dialogueHandler = dialogueHandler;

        // Call StartCutscene on the object
        currentObject.PlayCutscene();
    }
}

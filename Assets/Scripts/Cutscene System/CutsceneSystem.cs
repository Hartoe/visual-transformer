using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CutsceneSystem : MonoBehaviour
{
    [SerializeField] SceneChange sceneChange;
    public List<CutsceneStep> steps = new List<CutsceneStep>();
    public DialogueHandler dialogueHandler;

    void Start()
    {
        PlayCutscene();
    }

    public void PlayCutscene()
    {
        StartCoroutine(RunCutscene());
    }

    IEnumerator RunCutscene()
    {
        foreach (var step in steps)
        {
            ActivateCamera(step.stepCamera);
            yield return new WaitForSeconds(step.waitTime);

            if (step.highlightItems)
            {
                // Get list of coords
                List<GameObject> objects = new List<GameObject>();
                foreach (var coord in step.objectCoords)
                {
                    objects.Add(GridBuildingSystem.Instance.GetGrid().GetGridObject(coord.X, coord.Y).GetBuilding().gameObject);
                }
                HighlightObjects highlightObjects = gameObject.AddComponent<HighlightObjects>();
                highlightObjects.gameObjects = objects;
                highlightObjects.Highlight();
                yield return new WaitUntil(() => highlightObjects.Finished);
                Destroy(highlightObjects);
            }

            if (step.startDialogue)
            {
                Dialogue dialogue = gameObject.AddComponent<Dialogue>();
                dialogue.dialogueTextSO = step.dialogueSO;
                dialogue.dialogueHandler = dialogueHandler;
                dialogue.dialogueHandler.Finished = false;
                yield return new WaitUntil(() => dialogue.dialogueHandler.Finished);
                Destroy(dialogue);
            }
        }

        EndCutscene();
    }

    private void EndCutscene()
    {
        sceneChange.HandleLevelEnd();
    }

    private void ActivateCamera(CinemachineVirtualCamera stepCamera)
    {
        foreach (var step in steps)
        {
            if (step.stepCamera == stepCamera)
                stepCamera.Priority = 10;
            else
                step.stepCamera.Priority = 0;
        }
    }
}

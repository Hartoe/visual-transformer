using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public DialogueTextSO dialogueSO;
    public DialogueHandler dialogueHandler;
    public Animator transition;
    public float transitionTime = 1f;

    void Start()
    {
        ExportFactory.OnLevelComplete.AddListener(HandleLevelEnd);
    }

    private void HandleLevelEnd()
    {
        // Optional dialogue handling
        if (dialogueSO != null)
        {
            Dialogue dialogue = gameObject.AddComponent<Dialogue>();
            dialogue.dialogueTextSO = dialogueSO;
            dialogue.dialogueHandler = dialogueHandler;
            dialogue.dialogueHandler.Finished = false;
            dialogue.OnDialogueEnd.AddListener(LoadNextLevel);
        }
        else
        {
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }
}

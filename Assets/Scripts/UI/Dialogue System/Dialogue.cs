using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] DialogueTextSO dialogueTextSO;
    [SerializeField] DialogueHandler dialogueHandler;

    bool showFirst = true;

    void Start()
    {
        dialogueHandler.DisplayNextParagraph(dialogueTextSO);
    }

    void Update()
    {
        if (showFirst)
        {
            if (Input.GetMouseButtonDown(0))
            {
                dialogueHandler.DisplayNextParagraph(dialogueTextSO);
            }
            if (dialogueHandler.Finished) showFirst = false;
        }
    }

    public void InteractWithDialogue()
    {
        dialogueHandler.DisplayNextParagraph(dialogueTextSO);
    }
}

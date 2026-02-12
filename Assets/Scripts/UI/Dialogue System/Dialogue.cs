using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] DialogueTextSO dialogueTextSO;
    [SerializeField] DialogueHandler dialogueHandler;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InteractWithDialogue();
        }
    }

    public void InteractWithDialogue()
    {
        dialogueHandler.DisplayNextParagraph(dialogueTextSO);
    }
}

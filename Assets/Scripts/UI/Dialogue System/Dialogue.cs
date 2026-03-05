using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    [SerializeField] public DialogueTextSO dialogueTextSO;
    [SerializeField] public DialogueHandler dialogueHandler;
    public UnityEvent OnDialogueEnd = new UnityEvent();

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
            if (dialogueHandler.Finished)
            {
                showFirst = false;
                OnDialogueEnd.Invoke();
            }
        }
    }

    public void InteractWithDialogue()
    {
        dialogueHandler.DisplayNextParagraph(dialogueTextSO);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    public static DialogueHandler Instance;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private float typeSpeed = 15f;

    private Queue<string> dialogues = new Queue<string>();
    private bool hasEnded;
    private bool isTyping;
    private string currentLine;
    private Coroutine typeDialogueCoroutine;
    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public void DisplayNextParagraph(DialogueTextSO dialogueText)
    {
        if (dialogues.Count <= 0)
        {
            if(!hasEnded)
            {
                StartDialogue(dialogueText);
            }
            else if (hasEnded && !isTyping)
            {
                EndDialogue();
                return;
            }
        }

        if (!isTyping)
        {
            currentLine = dialogues.Dequeue();
            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(currentLine));
        }
        else
        {
            EarlyDialogueExit(currentLine);
        }

        if (dialogues.Count <= 0) hasEnded = true;
    }

    private void StartDialogue(DialogueTextSO dialogueText)
    {
        GridBuildingSystem.Instance.SetBuildActive(false);
        
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        characterNameText.text = dialogueText.characterName;
        for (int i = 0; i < dialogueText.dialogues.Length; i++)
        {
            dialogues.Enqueue(dialogueText.dialogues[i]);
        }
    }

    private void EndDialogue()
    {
        GridBuildingSystem.Instance.SetBuildActive(true);
        dialogues.Clear();
        hasEnded = false;
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    private IEnumerator TypeDialogueText(string currentLine)
    {
        isTyping = true;

        dialogueTextUI.text = "";

        string originalText = currentLine;
        string displayedText = "";
        int alphaIndex = 0;

        foreach (char c in currentLine.ToCharArray())
        {
            alphaIndex++;
            dialogueTextUI.text = originalText;

            displayedText = dialogueTextUI.text.Insert(alphaIndex, HTML_ALPHA);
            dialogueTextUI.text = displayedText;

            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false;
    }

    private void EarlyDialogueExit(string currentLine)
    {
        StopCoroutine(typeDialogueCoroutine);
        dialogueTextUI.text = currentLine;
        isTyping = false;
    }
}

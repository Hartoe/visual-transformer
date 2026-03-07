using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [SerializeField] private GameObject buildMenu;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private float typeSpeed = 15f;

    public bool Finished = false;

    private Queue<string> dialogues = new Queue<string>();
    private bool hasEnded;
    private bool isTyping;
    private string currentLine;
    private Coroutine typeDialogueCoroutine;
    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;
    private bool buildState;

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
        buildState= GridBuildingSystem.Instance.GetBuildActive();
        GridBuildingSystem.Instance.SetBuildActive(false);
        buildMenu.SetActive(false);
        
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
        GridBuildingSystem.Instance.SetBuildActive(buildState);
        buildMenu.SetActive(true);
        dialogues.Clear();
        hasEnded = false;
        Finished = true;
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

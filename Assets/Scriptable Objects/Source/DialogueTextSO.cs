using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DialogueTextSO : ScriptableObject
{
    public string characterName;
    [TextArea(5, 10)]
    public string[] dialogues;
}

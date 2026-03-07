using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CutsceneStep : MonoBehaviour
{
    [Serializable]
    public class Coord
    {
        public int X;
        public int Y;
    }
    public CinemachineVirtualCamera stepCamera;
    public float waitTime = 2.1f;

    public bool startDialogue;
    public DialogueTextSO dialogueSO;

    public bool highlightItems;
    public List<Coord> objectCoords;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintButton : MonoBehaviour
{
    [SerializeField] GameObject hintPanel;
    [SerializeField] Button button;

    void Start()
    {
        button.onClick.AddListener(ToggleHintPanel);
    }

    private void ToggleHintPanel()
    {
        hintPanel.SetActive(!hintPanel.activeSelf);
    }
}

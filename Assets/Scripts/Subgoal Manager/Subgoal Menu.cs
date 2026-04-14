using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubgoalMenu : MonoBehaviour
{
    public static SubgoalMenu Instance;

    [SerializeField] List<Toggle> toggles;
    [SerializeField] SubgoalManager manager;

    void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
    }

    void Start()
    {
        manager.OnGoalComplete.AddListener(UpdateMenu);
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].GetComponentInChildren<TextMeshProUGUI>().text = manager.GetGoal(i).Title;
        }  
    }

    private void UpdateMenu(int index)
    {
        toggles[index].isOn = true;
        toggles[index].GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
    }

    public void Reset()
    {
        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
            toggle.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
    }
}

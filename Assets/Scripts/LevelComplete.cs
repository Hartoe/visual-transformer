using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    [SerializeField] string nextScene;

    // Start is called before the first frame update
    void Start()
    {
        ExportFactory.OnLevelComplete.AddListener(GotoNextLevel);
    }

    private void GotoNextLevel()
    {
        SceneManager.LoadScene(nextScene);
    }
}

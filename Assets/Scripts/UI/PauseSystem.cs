using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSystem : MonoBehaviour
{
    [SerializeField] TabMenuButton resetButton;
    [SerializeField] TabMenuButton pauseButton;
    [SerializeField] TabMenuButton startButton;

    // Start is called before the first frame update
    void Start()
    {
        resetButton.OnTabSelected.AddListener(HandleReset);
        pauseButton.OnTabSelected.AddListener(HandlePause);
        startButton.OnTabSelected.AddListener(HandleStart);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pauseButton.SwitchTab();
            startButton.SwitchTab();
            TimeTickSystem.SetTimeTick(startButton.Selected);
        }
    }

    private void HandlePause(bool selected)
    {
        if (selected)
        {
            if (startButton.Selected)
            {
                startButton.SwitchTab();
            }

            TimeTickSystem.SetTimeTick(false);
        } else
        {
            if (!startButton.Selected)
            {
                startButton.SwitchTab();
            }
            TimeTickSystem.SetTimeTick(true);
        }
    }

    private void HandleStart(bool selected)
    {
        if (selected)
        {
            if (pauseButton.Selected)
            {
                pauseButton.SwitchTab();
            }

            TimeTickSystem.SetTimeTick(true);
        } else
        {
            if (!pauseButton.Selected)
            {
                pauseButton.SwitchTab();
            }
            TimeTickSystem.SetTimeTick(false);
        }
    }

    private void HandleReset(bool selected)
    {
        if (selected)
        {
            GridBuildingSystem.Instance.ResetGrid();
            resetButton.SwitchTab();
        }
    }
}

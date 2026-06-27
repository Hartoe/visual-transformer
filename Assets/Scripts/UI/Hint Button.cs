using UnityEngine;
using UnityEngine.EventSystems;
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

    void Update()
    {
        // Check if escape is pressed (close panel)
        if (Input.GetKeyDown(KeyCode.Escape) && hintPanel.activeSelf)
        {
            ToggleHintPanel();
        }

        // Check if mouse button is pressed over no UI element (close panel)
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && hintPanel.activeSelf)
        {
            ToggleHintPanel();
        }
    }
}

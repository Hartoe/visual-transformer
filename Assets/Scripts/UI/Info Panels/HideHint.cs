using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideHint : MonoBehaviour
{
    [SerializeField] GameObject HintButton;

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount > 0 && HintButton.activeSelf)
            HintButton.SetActive(false);
        else if (transform.childCount == 0 && !HintButton.activeSelf)
            HintButton.SetActive(true);

        // Check if escape is pressed (close panel)
        if (Input.GetKeyDown(KeyCode.Escape) && transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            HintButton.SetActive(true);
        }
    }
}

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
    }
}

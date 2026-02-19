using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorInfoPanel : MonoBehaviour
{
    public bool AddListener = true;
    public Button Button;
    [SerializeField] TextMeshProUGUI status;

    public void SetStatus(string _string)
    {
        status.text = $"Status: {_string}";
    }
}

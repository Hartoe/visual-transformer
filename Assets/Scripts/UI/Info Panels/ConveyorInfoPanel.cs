using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConveyorInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void SetText(string _string)
    {
        text.text = _string;
    }
}

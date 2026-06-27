using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrokenMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI message;
    public void SetMessage(string line)
    {
        message.text = line;
    }
}

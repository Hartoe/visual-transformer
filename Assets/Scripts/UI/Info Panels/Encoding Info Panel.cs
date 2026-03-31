using TMPro;
using UnityEngine;
using Utilities.ML;

public class EncodingInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void ResetText()
    {
        text.text = "Current Encoding:";
    }

    public void SetText(Matrix[] items)
    {
        foreach (Matrix item in items)
            text.text += $"\n{item}";
    }
}

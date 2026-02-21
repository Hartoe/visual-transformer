using TMPro;
using UnityEngine;

public class EncodingInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void ResetText()
    {
        text.text = "Current Encoding:";
    }

    public void SetText(Resource[] items)
    {
        foreach (Resource item in items)
            text.text += $"\n{item.state}";
    }
}

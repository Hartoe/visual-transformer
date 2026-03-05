using TMPro;
using UnityEngine;

public class EncodingInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void ResetText()
    {
        text.text = "Current Encoding:";
    }

    public void SetText(WorldItem[] items)
    {
        foreach (WorldItem item in items)
            text.text += $"\n{item.state}";
    }
}

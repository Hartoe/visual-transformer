using TMPro;
using UnityEngine;

public class EmbeddingInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void ResetText()
    {
        text.text = "Current Embedding:";
    }

    public void SetText(Resource[] items)
    {
        foreach (Resource item in items)
            text.text += $"\n{item.state}";
    }
}

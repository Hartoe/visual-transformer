using TMPro;
using UnityEngine;

public class PassInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void SetText(WorldItem item)
    {
        if (item == null)
        {
            text.text = $"Current Item: None";
        } else
        {
            text.text = $"Current Item: {item.name.Split('(')[0]}\n{item.state}";
        }
    }
}

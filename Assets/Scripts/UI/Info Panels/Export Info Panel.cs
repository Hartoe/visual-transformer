using TMPro;
using UnityEngine;
using Utilities.ML;

public class OutputInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI output;
    [SerializeField] TextMeshProUGUI status;

    public void SetText(Matrix? matrix, bool complete = false)
    {
        if (matrix == null)
        {
            output.text = $"Output: None";
            status.text = "Status: Waiting";
        } else
        {
            output.text = $"Output: \n{matrix}";
            if (complete)
                status.text = "Status: Correct!";
            else    
                status.text = "Status: Incorrect!";
        }
    }
}

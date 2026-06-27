
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.ML;

public class MathInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI matrixAText;
    [SerializeField] TextMeshProUGUI matrixBText;
    public Button Button;
    public bool AddListener = true;

    public void SetMatrixA(Matrix matrix)
    {
        matrixAText.text = $"Matrix A:\n{matrix}";
    }
    public void SetMatrixA()
    {
        matrixAText.text = "Matrix A:";
    }

    public void SetMatrixB(Matrix matrix)
    {
        matrixBText.text = $"Matrix B:\n{matrix}";
    }
    public void SetMatrixB()
    {
        matrixBText.text = "Matrix B:";
    }
}

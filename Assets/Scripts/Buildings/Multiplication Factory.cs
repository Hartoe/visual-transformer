using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class MultiplicationFactory : MultiInputFactory
{
    [SerializeField] Intent itemPrefab;
    
    new void Start()
    {
        TimeTickSystem.OnTick += UpdateInfoPanel;
        base.Start();
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (setFlags[0].Item1 && setFlags[1].Item1)
        {
            try
            {
                Matrix C = ((Matrix)setFlags[0].Item2) * ((Matrix)setFlags[1].Item2);
                Intent output = Instantiate(itemPrefab, Center, Quaternion.identity);
                output.state = C;
                ClearMatrices();
                outputs.Add(output);
            }
            catch
            {
                Break("Incompatible matrix dimensions for multiplication!");
            }
        }
    }

    private void UpdateInfoPanel(object sender, TimeTickSystem.TickEventArgs e)
    {
        UpdateInfoPanel();
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            MathInfoPanel panel = infoPanelInstance.GetComponent<MathInfoPanel>();
            if (setFlags[0].Item2 != null)
                panel.SetMatrixA((Matrix)setFlags[0].Item2);
            else
                panel.SetMatrixA();
            if (setFlags[1].Item2 != null)
                panel.SetMatrixB((Matrix)setFlags[1].Item2);
            else
                panel.SetMatrixB();

            if (panel.AddListener)
            {
                panel.Button.onClick.AddListener(ClearMatrices);
                panel.AddListener = false;
            }
        }
    }

    private void ClearMatrices()
    {
        Reset();
        UpdateInfoPanel();
    }

    new void OnDestroy()
    {
        TimeTickSystem.OnTick -= UpdateInfoPanel;
        foreach (Intent item in outputs)
            Destroy(item.gameObject);
        base.OnDestroy();
    }
}

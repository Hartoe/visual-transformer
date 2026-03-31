using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.ML;

public class ActivationFactory : PassThroughFactory
{
    [SerializeField] Intent itemPrefab;
    public Activation.ActivationType activationType;
    private IActivation activationFunction;

    new void Start()
    {
        activationFunction = Activation.GetActivationFromType(activationType);
        base.Start();
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (inputs.Count > 0)
        {
            Matrix input = inputs.First();
            inputs.RemoveAt(0);
            Matrix output = new Matrix(input.Shape);
            for (int i = 0; i < output.Rows; i++)
            {
                for (int j = 0; j < output.Columns; j++)
                {
                    output[i,j] = activationFunction.Activate(input, i, j);
                }
            }
            WorldItem newItem = Instantiate(itemPrefab, Center, Quaternion.identity);
            newItem.state = output;
            outputs.Add(newItem);
        }
    }

    protected override void UpdateInfoPanel()
    {
        if (infoPanelInstance != null)
        {
            ActivationInfoPanel panel = infoPanelInstance.GetComponent<ActivationInfoPanel>();
            if ((int)activationType != panel.Dropdown.value)
            {
                panel.Dropdown.value = (int)activationType;
            }
            switch(activationType)
            {
                default:
                case Activation.ActivationType.Sigmoid:
                    panel.Function.sprite = panel.SigmoidFunction;
                    break;
                case Activation.ActivationType.TanH:
                    panel.Function.sprite = panel.TanHFunction;
                    break;
                case Activation.ActivationType.ReLU:
                    panel.Function.sprite = panel.ReLUFunction;
                    break;
                case Activation.ActivationType.SiLU:
                    panel.Function.sprite = panel.SiLUFunction;
                    break;
                case Activation.ActivationType.Softmax:
                    panel.Function.sprite = panel.SoftmaxFunction;
                    break;
            }

            panel.Dropdown.onValueChanged.AddListener(ChangeActivatorFunction);
        }
    }

    private void ChangeActivatorFunction(int value)
    {
        activationType = (Activation.ActivationType)value;
        activationFunction = Activation.GetActivationFromType(activationType);
        UpdateInfoPanel();
    }
}

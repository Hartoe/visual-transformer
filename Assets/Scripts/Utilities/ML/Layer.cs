using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {
#region Layer
        public class Layer
        {
            public readonly Matrix weights;
            public readonly Matrix biases;

            public readonly Matrix costGradientW;
            public readonly Matrix costGradientB;

            public readonly Matrix weightsVelocity;
            public readonly Matrix biasesVelocity;

            public IActivation activation;

#region Constructors
            public Layer((int, int) sizeIn, (int, int) sizeOut)
            {
                activation = new Activation.Sigmoid();

                weights = new Matrix(sizeIn.Item2, sizeOut.Item2);
                costGradientW = new Matrix(sizeIn.Item2, sizeOut.Item2);
                weightsVelocity = new Matrix(sizeIn.Item2, sizeOut.Item2);

                biases = new Matrix(sizeOut);
                costGradientB = new Matrix(sizeOut);
                biasesVelocity = new Matrix(sizeOut);

                InitializeRandomWeights();
            }
            public Layer(int sizeIn, int sizeOut)
            {
                activation = new Activation.Sigmoid();

                weights = new Matrix(sizeIn, sizeOut);
                costGradientW = new Matrix(sizeIn, sizeOut);
                weightsVelocity = new Matrix(sizeIn, sizeOut);

                biases = new Matrix(sizeOut, 1);
                costGradientB = new Matrix(sizeOut, 1);
                biasesVelocity = new Matrix(sizeOut, 1);

                InitializeRandomWeights();
            }
#endregion

#region Learning
            public Matrix CalculateOutputs(Matrix inputs)
            {
                Matrix weightedInputs = (inputs * weights) + biases;

                Matrix activations = new Matrix(biases.Rows, biases.Columns);
                for (int i = 0; i < activations.Rows; i++)
                {
                    for (int j = 0; j < activations.Columns; j++)
                    {
                        activations[i, j] = activation.Activate(weightedInputs, i, j);
                    }
                }
                return activations;
            }

            public Matrix CalculateOutputs(Matrix inputs, LayerLearnData learnData)
            {
                learnData.inputs = inputs;
                learnData.weightedInputs = (inputs * weights) + biases;

                for (int i = 0; i < learnData.activations.Rows; i++)
                {
                    for (int j = 0; j < learnData.activations.Columns; j++)
                    {
                        learnData.activations[i, j] = activation.Activate(learnData.weightedInputs, i, j);
                    }
                }
                return learnData.activations;
            }

            public void ApplyGradients(double learnRate, double regularization, double momentum)
            {
                double weightDecay = (1 - regularization * learnRate);

                for (int i = 0; i < weights.Rows; i++)
                {
                    for (int j = 0; j < weights.Columns; j++)
                    {
                        double weight = weights[i, j];
                        double velocity = weightsVelocity[i, j] * momentum - costGradientW[i, j] * learnRate;
                        weightsVelocity[i, j] = velocity;
                        weights[i, j] = weight * weightDecay + velocity;
                        costGradientW[i, j] = 0;
                    }
                }

                for (int i = 0; i < biases.Rows; i++)
                {
                    for (int j = 0; j < biases.Columns; j++)
                    {
                        double velocity = biasesVelocity[i, j] * momentum - costGradientB[i, j] * learnRate;
                        biasesVelocity[i, j] = velocity;
                        biases[i, j] += velocity;
                        costGradientB[i, j] = 0;
                    }
                }
            }

            public void CalculateOutputLayerNodeValues(LayerLearnData learnData, Matrix expectedOutputs, ICost cost)
            {
                for (int i = 0; i < learnData.nodeValues.Rows; i++)
                {
                    for (int j = 0; j < learnData.nodeValues.Columns; j++)
                    {
                        double costDerivative = cost.CostDerivative(learnData.activations[i, j], expectedOutputs[i, j]);
                        double activationDerivative = activation.Derivative(learnData.weightedInputs, i, j);
                        learnData.nodeValues[i, j] = costDerivative * activationDerivative;
                    }
                }
            }

            public void CalculateHiddenLayerNodeValues(LayerLearnData learnData, Layer oldLayer, Matrix oldNodeValues)
            {
                throw new NotImplementedException();
            }
#endregion

            private void InitializeRandomWeights()
            {
                for (int i = 0; i < weights.Rows; i++)
                {
                    for (int j = 0; j < weights.Columns; j++)
                    {
                        weights[i, j] = RandomInNormalDistribution(0, 1) / Math.Sqrt(weights.Rows);
                    }
                }

                double RandomInNormalDistribution(double mean, double standardDeviation)
                {
                    double x1 = 1 - UnityEngine.Random.value;
                    double x2 = 1 - UnityEngine.Random.value;

                    double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
                    return y1 * standardDeviation + mean;
                }
            }
        }
#endregion
    }
}

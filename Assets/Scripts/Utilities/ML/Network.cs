using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {

#region Fully-Connected NN
        public class FullyConnectedNN : INetwork
        {
            public readonly Layer[] layers;
            public readonly (int, int)[] layerSizes;

            public ICost cost;
            NetworkLearnData[] batchLearnData;

            public FullyConnectedNN(params (int, int)[] layerSizes)
            {
                this.layerSizes = layerSizes;
                layers = new Layer[layerSizes.Length - 1];
                for (int i = 0; i < layers.Length; i++)
                {
                    layers[i] = new Layer(layerSizes[i], layerSizes[i + 1]);
                }
                cost = new Cost.MeanSquaredError();
            }

            public Matrix CalculateOutputs(Matrix inputs)
            {
                foreach (Layer layer in layers)
                {
                    inputs = layer.CalculateOutputs(inputs);
                }
                return inputs;
            }

            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                if (batchLearnData == null || batchLearnData.Length != trainingData.Length)
                {
                    batchLearnData = new NetworkLearnData[trainingData.Length];
                    for (int i = 0; i < batchLearnData.Length; i++)
                    {
                        batchLearnData[i] = new NetworkLearnData(layers);
                    }
                }

                // TODO: MAKE PARALLEL SAFE
                for (int i = 0; i < trainingData.Length; i++)
                {
                    UpdateGradients(trainingData[i], batchLearnData[i]);
                }

                for (int i = 0; i < layers.Length; i++)
                {
                    layers[i].ApplyGradients(learnRate / trainingData.Length, regularization, momentum);
                }
            }

            private void UpdateGradients(DataPoint data, NetworkLearnData learnData)
            {
                Matrix inputsToNextLayer = data.inputs;

                for (int i = 0; i < layers.Length; i++)
                {
                    inputsToNextLayer = layers[i].CalculateOutputs(inputsToNextLayer, learnData.layerData[i]);
                }

                // Backpropagation
                int outputLayerIndex = layers.Length - 1;
                Layer outputLayer = layers[outputLayerIndex];
                LayerLearnData outputLearnData = learnData.layerData[outputLayerIndex];

                outputLayer.CalculateOutputLayerNodeValues(outputLearnData, data.expectedOutputs, cost);
                outputLayer.UpdateGradients(outputLearnData);
                
                for (int i = outputLayerIndex - 1; i >= 0; i--)
                {
                    LayerLearnData layerLearnData = learnData.layerData[i];
                    Layer hiddenLayer = layers[i];

                    hiddenLayer.CalculateHiddenLayerNodeValues(layerLearnData, layers[i + 1], learnData.layerData[i + 1].nodeValues);
                    hiddenLayer.UpdateGradients(layerLearnData);
                }
            }
            
            public void SetCostFunction(ICost costFunction) => this.cost = costFunction;
            public void SetActivationFunction(IActivation activation) => SetActivationFunction(activation, activation);
            public void SetActivationFunction(IActivation activation, IActivation outputActivation)
            {
                for (int i = 0; i < layers.Length - 1; i++)
                    layers[i].SetActivationFunction(activation);
                layers[layers.Length - 1].SetActivationFunction(outputActivation);
            }
        }
#endregion

#region Positional Embedding
        public class PositionalEmbedding : INetwork
        {
            public PositionalEmbedding(){}

            public Matrix CalculateOutputs(Matrix inputs)
            {
                Matrix posEncoding = GetPositionalEncoding(inputs);
                return inputs + posEncoding;
            }

            // Doesn't have to learn, just pass outputs into training data structure for next part of the model
            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                // Just add position to data.inputs and pass on
                throw new NotImplementedException();
            }

            private Matrix GetPositionalEncoding(Matrix inputs, double n = 10000)
            {
                Matrix result = new Matrix((inputs.Rows, inputs.Columns));
                double dimension = inputs.Columns;
                for (int k = 0; k < inputs.Rows; k++)
                {
                    for (int i = 0; i < inputs.Columns/2; i++)
                    {
                        double denominator = Math.Pow(n, 2*i  / dimension);
                        double sinPos = Math.Sin(k/denominator);
                        double cosPos = Math.Cos(k/denominator);
                        result[k, 2*i] = sinPos;
                        result[k, 2*i + 1] = cosPos;
                    }
                }
                return result;
            }
        }
#endregion

#region Layer Normalization
        public class LayerNorm : INetwork
        {
            public readonly Matrix scaleMatrix;
            public readonly Matrix shiftMatrix;

            public LayerNorm((int, int) size)
            {
                scaleMatrix = new Matrix(size);
                scaleMatrix.Fill(1);
                shiftMatrix = new Matrix(size);
                shiftMatrix.Fill(0);
            }
            public LayerNorm(int rows, int cols)
            {
                scaleMatrix = new Matrix(rows, cols);
                shiftMatrix = new Matrix(rows, cols);
            }

            public Matrix CalculateOutputs(Matrix inputs)
            {
                // Calculate mean and deviation
                double mean = 0;
                double deviation = 0;
                double reciprical = 1.0 / (inputs.Rows*inputs.Columns);
                double epsilon = 0.00001;

                for (int i = 0; i < inputs.Rows; i++)
                {
                    for (int j = 0; j < inputs.Columns; j++)
                    {
                        mean += inputs[i,j];
                    }
                }
                mean *= reciprical;

                for (int i = 0; i < inputs.Rows; i++)
                {
                    for (int j = 0; j < inputs.Columns; j++)
                    {
                        deviation += Math.Pow(inputs[i,j] - mean, 2);
                    }
                }
                deviation *= reciprical;

                // Apply normalization
                for (int i = 0; i < inputs.Rows; i++)
                {
                    for (int j = 0; j < inputs.Columns; j++)
                    {
                        inputs[i,j] = (inputs[i,j] - mean) / Math.Sqrt(deviation + epsilon);
                    }
                }

                // Apply scaling and shifting
                return Matrix.LinMult(inputs, scaleMatrix) + shiftMatrix;
            }
            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                
            }
        }
#endregion

#region Attention Head
        public class Attention : INetwork
        {
            public Matrix Queries;
            public Matrix Keys;
            public Matrix Values;

            public Layer queryLayer;
            public Layer keyLayer;
            public Layer valueLayer;

            public Matrix CalculateOutputs(Matrix inputs)
            {
                // Caluclate the query, key, and value matrices
                Queries = queryLayer.CalculateOutputs(inputs);
                Keys = keyLayer.CalculateOutputs(inputs);
                Values = valueLayer.CalculateOutputs(inputs);

                // Calculate query/key properties
                Matrix QKs = Queries * Keys;

                // Scale and softmax

                // Matmult with value matrix

                // 
                throw new NotImplementedException();
            }
            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                
            }
        }
#endregion

#region Skip Connection
        public class SkipConnection : INetwork
        {
            public Matrix? savedInput;

            public SkipConnection() {}

            public Matrix CalculateOutputs(Matrix inputs)
            {
                if (savedInput == null)
                {
                    savedInput = inputs;
                    return inputs;
                }
                else
                {
                    return inputs + (Matrix)savedInput;
                }
            }
            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                
            }
        }
#endregion

#region Interface
        public interface INetwork
        {
            public Matrix CalculateOutputs(Matrix inputs);
            public void Learn(DataPoint[] trainingData, double learnRate, double regularization = 0, double momentum = 0);
        }
#endregion

    }
}
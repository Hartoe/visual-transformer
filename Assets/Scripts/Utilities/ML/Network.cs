namespace Utilities
{
    namespace ML
    {
        public class NeuralNetwork
        {
            public readonly Layer[] layers;
            public readonly (int, int)[] layerSizes;

            public ICost cost;
            NetworkLearnData[] batchLearnData;

            public NeuralNetwork(params (int, int)[] layerSizes)
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

    }
}
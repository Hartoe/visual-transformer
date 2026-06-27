namespace Utilities
{
    namespace ML
    {
#region Learning Data

        public class LayerLearnData
        {
            public Matrix inputs;
            public Matrix weightedInputs;
            public Matrix activations;
            public Matrix nodeValues;

            public LayerLearnData(Layer layer)
            {
                (int, int) size = (layer.biases.Rows, layer.biases.Columns);
                weightedInputs = new Matrix(size);
                activations = new Matrix(size);
                nodeValues = new Matrix(size);
            }
        }

        public class NetworkLearnData
        {
            public LayerLearnData[] layerData;

            public NetworkLearnData(Layer[] layers)
            {
                layerData = new LayerLearnData[layers.Length];
                for (int i = 0; i < layerData.Length; i++)
                {
                    layerData[i] = new LayerLearnData(layers[i]);
                }
            }
        }

        public class DataPoint
        {
            public readonly Matrix inputs;
            public readonly Matrix expectedOutputs;

            public DataPoint(Matrix inputs, Matrix expectedOutputs)
            {
                this.inputs = inputs;
                this.expectedOutputs = expectedOutputs;
            }
        }

#endregion
    }
}
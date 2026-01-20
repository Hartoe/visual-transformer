using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {

#region Matrix
        public readonly struct Matrix
        {
            private readonly double[] _data;

            public int Rows { get; }
            public int Columns { get; }
            public (int, int) Shape { get { return (Rows, Columns); } }

            public static Matrix Identity(int size)
            {
                Matrix m = new Matrix(size, size);
                for (int i = 0; i < size; i++)
                    m._data[i * size + i] = 1.0;
                return m;
            }

#region Constructors
            public Matrix(int rows, int columns)
            {
                if (rows <= 0 || columns <= 0)
                    throw new ArgumentOutOfRangeException();

                Rows = rows;
                Columns = columns;
                _data = new double[rows * columns];
            }

            public Matrix((int, int) size)
            {
                if (size.Item1 <= 0 || size.Item2 <= 0)
                    throw new ArgumentOutOfRangeException();

                Rows = size.Item1;
                Columns = size.Item2;
                _data = new double[size.Item1 * size.Item2];
            }

            public Matrix(double[,] data)
            {
                Rows = data.GetLength(0);
                Columns = data.GetLength(1);
                _data = new double[Rows * Columns];

                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Columns; j++)
                        _data[i*Columns + j] = data[i, j];
            }

            public Matrix(double[] data)
            {
                Rows = data.Length;
                Columns = 1;
                _data = (double[])data.Clone();
            }
#endregion

            public double this[int row, int column]
            {
                get => _data[row * Columns + column];
                set => _data[row * Columns + column] = value;
            }
            public double this[int row]
            {
                get => _data[row];
                set => _data[row] = value;
            }

#region Operators
            // Dot product of two matrices
            public static Matrix operator *(Matrix left, Matrix right)
            {
                if (left.Columns != right.Rows)
                    throw new InvalidOperationException("Invalid matrix dimensions for multiplication!");

                int rowsLeft = left.Rows;
                int colsLeft = left.Columns;
                int colsRight = right.Columns;

                Matrix result = new Matrix(rowsLeft, colsRight);

                double [] dataLeft = left._data;
                double [] dataRight = right._data;
                double [] dataResult = result._data;

                for (int i = 0; i < rowsLeft; i++)
                {
                    int leftRow = i * colsLeft;
                    int resultRow = i * colsRight;

                    for (int k = 0; k < colsLeft; k++)
                    {
                        double leftK = dataLeft[leftRow + k];
                        int rightRow = k * colsRight;

                        for (int j = 0; j < colsRight; j++)
                            dataResult[resultRow + j] += leftK * dataRight[rightRow + j];
                    }
                }

                return result;
            }
            // Parallel dot product of two matrices
            public static Matrix ParallelDot(Matrix left, Matrix right)
            {
                if (left.Columns != right.Rows)
                    throw new InvalidOperationException("Invalid matrix dimensions for multiplication!");

                int rowsLeft = left.Rows;
                int colsLeft = left.Columns;
                int colsRight = right.Columns;

                Matrix result = new Matrix(rowsLeft, colsRight);

                double [] dataLeft = left._data;
                double [] dataRight = right._data;
                double [] dataResult = result._data;

                Parallel.For(0, rowsLeft, i =>
                {
                    int leftRow = i * colsLeft;
                    int resultRow = i * colsRight;

                    for (int k = 0; k < colsLeft; k++)
                    {
                        double leftK = dataLeft[leftRow + k];
                        int rightRow = k * colsRight;

                        for (int j = 0; j < colsRight; j++)
                            dataResult[resultRow + j] += leftK * dataRight[rightRow + j];
                    }
                });

                return result;
            }
            // Scalar multiply of matrix
            public static Matrix operator *(double scalar, Matrix matrix)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = scalar * matrix[i, j];
                return result;
            }
            public static Matrix operator *(Matrix matrix, double scalar) => scalar * matrix;

            // Scalar division of matrix
            public static Matrix operator /(Matrix matrix, double scalar)
            {
                return (1/scalar) * matrix;
            }

            // Addition of two matrices
            public static Matrix operator +(Matrix left, Matrix right)
            {
                int rows = left.Rows;
                int cols = left.Columns;
                if (rows != right.Rows || cols != right.Columns)
                    throw new InvalidOperationException("Invalid matrix dimensions for addition!");

                Matrix result = new Matrix(rows, cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j ++)
                        result[i, j] = left[i, j] + right[i, j];
                return result;
            }
            // Scalar addition of matrix
            public static Matrix operator +(double scalar, Matrix matrix)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = scalar + matrix[i, j];
                return result;
            }
            public static Matrix operator +(Matrix matrix, double scalar) => scalar + matrix;

            // Subtraction of two matrices
            public static Matrix operator -(Matrix left, Matrix right)
            {
                int rows = left.Rows;
                int cols = left.Columns;
                if (rows != right.Rows || cols != right.Columns)
                    throw new InvalidOperationException("Invalid matrix dimensions for subtraction!");

                Matrix result = new Matrix(rows, cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j ++)
                        result[i, j] = left[i, j] - right[i, j];
                return result;
            }
            // Scalar subtraction of matrix
            public static Matrix operator -(Matrix matrix, double scalar)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = matrix[i, j] - scalar;
                return result;
            }
#endregion

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                        sb.Append(this[i, j].ToString("0.###")).Append('\t');
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }
#endregion
        
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

#endregion

#region Activation Functions
        public readonly struct Activation
        {
            public enum ActivationType
            {
                Sigmoid,
                TanH,
                ReLU,
                SiLU,
                Softmax
            }

            public static IActivation GetActivationFromType(ActivationType type)
	        {
	        	switch (type)
	        	{
	        		case ActivationType.Sigmoid:
	        			return new Sigmoid();
	        		case ActivationType.TanH:
	        			return new TanH();
	        		case ActivationType.ReLU:
	        			return new ReLU();
	        		case ActivationType.SiLU:
	        			return new SiLU();
	        		case ActivationType.Softmax:
	        			return new Softmax();
	        		default:
	        			Debug.LogError("Unhandled activation type");
	        			return new Sigmoid();
	        	}
	        }

#region Activators
            public readonly struct Sigmoid : IActivation
            {
                public double Activate(Matrix inputs, int row, int col)
                {
                    return 1.0 / (1 + Math.Exp(-inputs[row, col]));
                }

                public double Derivative(Matrix inputs, int row, int col)
                {
                    double a = Activate(inputs, row, col);
			        return a * (1 - a);
                }

                public ActivationType GetActivationType()
                {
                    return ActivationType.Sigmoid;
                }
            }

            public readonly struct TanH : IActivation
            {
                public double Activate(Matrix inputs, int row, int col)
                {
                    double e2 = Math.Exp(2 * inputs[row, col]);
			        return (e2 - 1) / (e2 + 1);
                }

                public double Derivative(Matrix inputs, int row, int col)
                {
                    double e2 = Math.Exp(2 * inputs[row, col]);
			        double t = (e2 - 1) / (e2 + 1);
			        return 1 - t * t;
                }

                public ActivationType GetActivationType()
                {
                    return ActivationType.TanH;
                }
            }

            public readonly struct ReLU : IActivation
            {
                public double Activate(Matrix inputs, int row, int col)
                {
                    return Math.Max(0, inputs[row, col]);
                }

                public double Derivative(Matrix inputs, int row, int col)
                {
                    return (inputs[row, col] > 0) ? 1 : 0;
                }

                public ActivationType GetActivationType()
                {
                    return ActivationType.ReLU;
                }
            }

            public readonly struct SiLU : IActivation
            {
                public double Activate(Matrix inputs, int row, int col)
                {
                    return inputs[row, col] / (1 + Math.Exp(-inputs[row, col]));
                }

                public double Derivative(Matrix inputs, int row, int col)
                {
                    double sig = 1 / (1 + Math.Exp(-inputs[row, col]));
			        return inputs[row, col] * sig * (1 - sig) + sig;
                }

                public ActivationType GetActivationType()
                {
                    return ActivationType.SiLU;
                }
            }

            public readonly struct Softmax : IActivation
            {
                public double Activate(Matrix inputs, int row, int col)
                {
                    double expSum = 0;
			        for (int i = 0; i < inputs.Rows; i++)
			        {
			        	for (int j = 0; j < inputs.Columns; j++)
                        {
                            expSum += Math.Exp(inputs[i, j]);
                        }
			        }

			        double res = Math.Exp(inputs[row, col]) / expSum;

			        return res;
                }

                public double Derivative(Matrix inputs, int row, int col)
                {
                    double expSum = 0;
			        for (int i = 0; i < inputs.Rows; i++)
			        {
			        	for (int j = 0; j < inputs.Columns; j++)
                        {
                            expSum += Math.Exp(inputs[i, j]);
                        }
			        }

			        double ex = Math.Exp(inputs[row, col]);

			        return (ex * expSum - ex * ex) / (expSum * expSum);
                }

                public ActivationType GetActivationType()
                {
                    return ActivationType.Softmax;
                }
            }
        }
#endregion

#region Interface
        public interface IActivation
        {
            double Activate(Matrix inputs, int row, int col);
            double Derivative(Matrix inputs, int row, int col);

            Activation.ActivationType GetActivationType();
        }
#endregion

#endregion

#region Costs

        public class Cost
        {
            public enum CostType
            {
                MeanSquaredError,
                CrossEntropy
            }

            public static ICost GetCostFromType(CostType type)
            {
                switch(type)
                {
                    case CostType.MeanSquaredError:
                        return new MeanSquaredError();
                    case CostType.CrossEntropy:
                        return new CrossEntropy();
                    default:
                        Debug.LogError("Unhandled cost type");
                        return new MeanSquaredError();
                }
            }

#region Cost Functions

            public class MeanSquaredError : ICost
            {
                public double CostDerivative(double predictedOutput, double expectedOutput)
                {
                    return predictedOutput - expectedOutput;
                }

                public double CostFunction(Matrix predictedOutputs, Matrix expectedOutputs)
                {
                    double cost = 0;
                    for (int i = 0; i < predictedOutputs.Rows; i++)
                    {
                        for (int j = 0; j < predictedOutputs.Columns; j++)
                        {
                            double error = predictedOutputs[i, j] - expectedOutputs[i, j];
                            cost += error * error;
                        }
                    }
                    return 0.5 * cost;
                }

                public CostType CostFunctionType()
                {
                    return CostType.MeanSquaredError;
                }
            }

            public class CrossEntropy : ICost
            {
                public double CostDerivative(double predictedOutput, double expectedOutput)
                {
                    double x = predictedOutput;
                    double y = expectedOutput;
                    if (x == 0 || x == 1)
                    {
                        return 0;
                    }
                    return (-x + y) / (x * (x - 1));
                }

                public double CostFunction(Matrix predictedOutputs, Matrix expectedOutputs)
                {
                    double cost = 0;
                    for (int i = 0; i < predictedOutputs.Rows; i++)
                    {
                        for (int j = 0; j < predictedOutputs.Columns; j++)
                        {
                            double x = predictedOutputs[i, j];
                            double y = expectedOutputs[i, j];
                            double v = (y == 1) ? -Math.Log(x) : -Math.Log(1 - x);
                            cost += double.IsNaN(v) ? 0 : v;
                        }
                    }
                    return cost;
                }

                public CostType CostFunctionType()
                {
                    return CostType.CrossEntropy;
                }
            }

#endregion
        }

#region Interface
        public interface ICost
        {
            double CostFunction(Matrix predictedOutputs, Matrix expectedOutputs);

            double CostDerivative(double predictedOutput, double expectedOutput);

            Cost.CostType CostFunctionType();
        }
#endregion

#endregion

    }
}
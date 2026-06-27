using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {
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
    }
}

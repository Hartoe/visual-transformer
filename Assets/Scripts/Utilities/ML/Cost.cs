using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {
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

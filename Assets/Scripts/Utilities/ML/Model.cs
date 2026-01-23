using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {
        public class Model : INetwork
        {
            public readonly List<INetwork> components;

            public Model()
            {
                components = new List<INetwork>();
            }

            public void Add(INetwork component) => components.Add(component);

            public Matrix CalculateOutputs(Matrix inputs)
            {
                foreach(INetwork cmp in components)
                {
                    inputs = cmp.CalculateOutputs(inputs);
                }
                return inputs;
            }

            public void Learn(DataPoint[] traindingData, double learnRate, double regularization = 0, double momentum = 0)
            {
                // Split up data in batches

                // For each batch:
                // * Run Learn() for each component in model
                // -> TrainingData contains array of suited DataPoints per component (?)
            }
        }
    }
}

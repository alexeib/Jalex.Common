using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics;

namespace Jalex.MachineLearning.Extractors
{
    internal class InputBuilder<TInput>
    {
        private readonly IInputExtractor<TInput> _inputExtractor;

        public InputBuilder(IInputExtractor<TInput> inputExtractor)
        {
            _inputExtractor = inputExtractor;
        }

        public double[][] BuildInputs(IEnumerable<TInput> inputs)
        {
            var doubles = doubleInputs(inputs);
            double[][] numericalInputs = doubles.ToArray();
            return numericalInputs;
        }

        public Tuple<double, double>[] NormalizeInputs(double[][] inputs)
        {
            return null;

            var meanStd = new Tuple<double, double>[inputs[0].Length];

            var numParameters = inputs[0].Length;
            for (int i = 0; i < numParameters; i++)
            {
                var samples = inputs.Select(x => x[i])
                                    .ToArray();
                var mean = samples.Mean();
                var std = samples.StandardDeviation(mean);

                if (!double.IsNaN(std))
                {
                    foreach (double[] input in inputs)
                    {
                        input[i] = (input[i] - mean) / std;
                    }
                }

                meanStd[i] = new Tuple<double, double>(mean, std);
            }

            return meanStd;
        }

        private IEnumerable<double[]> doubleInputs(IEnumerable<TInput> inputs)
        {
            return from input in inputs
                   let numericalInputs = _inputExtractor.ExtractInputs(input)
                   where inputs != null
                   select numericalInputs;
        }
    }
}

using System;
using System.Linq;
using Accord.Statistics;

namespace Jalex.MachineLearning.Extractors
{
    internal class DoubleInputBuilder<TInput>: InputBuilder<TInput, double>
    {
        public DoubleInputBuilder(IInputExtractor<TInput, double> inputExtractor)
            : base(inputExtractor)
        {
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
    }
}

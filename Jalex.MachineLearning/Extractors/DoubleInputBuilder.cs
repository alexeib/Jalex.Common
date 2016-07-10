using System;
using System.Diagnostics;
using System.Linq;
using Accord.Statistics;
using Jalex.Infrastructure.Extensions;

namespace Jalex.MachineLearning.Extractors
{
    internal class DoubleInputBuilder<TInput> : InputBuilder<TInput, double>
    {
        public DoubleInputBuilder(IInputExtractor<TInput, double> inputExtractor)
            : base(inputExtractor)
        {
        }

        public NormalizationParams[] NormalizeInputs(double[][] inputs)
        {
            //return null;

            var normPs = new NormalizationParams[inputs[0].Length];

            var numParameters = inputs[0].Length;
            for (int i = 0; i < numParameters; i++)
            {
                var samples = inputs.Select(x => x[i])
                                    .ToArray();

                var sampleMean = samples.Mean();
                var sampleStd = samples.StandardDeviation(sampleMean);
                samples = samples.Where(x => x > sampleMean + sampleStd * 2 || x < sampleMean - sampleStd * 2)
                                 .ToArray();

                var min = samples.Min();
                var max = samples.Max();

                var scaledSamples = samples.Select(x => scale(x, min, max))
                                           .ToArray();

                var mean = scaledSamples.Mean();
                var std = scaledSamples.StandardDeviation(mean);

                normPs[i] = new NormalizationParams
                {
                    Min = min,
                    Max = max,
                    ScaledMean = mean,
                    ScaledStd = std,
                };

                foreach (double[] input in inputs)
                {
                    input[i] = normPs[i].Normalize(input[i]);
                }

                
            }

            return normPs;
        }

        private double scale(double d, double min, double max)
        {
            return (d - min) / (max - min);
        }
    }
}

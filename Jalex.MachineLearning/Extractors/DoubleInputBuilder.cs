using System.Linq;
using Accord.Statistics;
using Jalex.Infrastructure.Extensions;

namespace Jalex.MachineLearning.Extractors
{
    internal class DoubleInputBuilder<TInput> : InputBuilder<TInput, double>
    {
        const double _stdsToClip = 2;
        const double _desiredMin = -1, _desiredMax = 1;

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
                samples = samples.Where(x => x < sampleMean + sampleStd * _stdsToClip && x > sampleMean - sampleStd * _stdsToClip)
                                 .ToArray();

                //sampleMean = samples.Mean();
                //sampleStd = samples.StandardDeviation(sampleMean);

                //var centeredSamples = samples.Select(x => NormalizationParams.Standardize(x, sampleMean, sampleStd))
                //                             .ToCollection();

                //var min = centeredSamples.Min();
                //var max = centeredSamples.Max();

                var min = samples.Min();
                var max = samples.Max();

                normPs[i] = new NormalizationParams
                {
                    FromMin = min,
                    FromMax = max,
                    ToMin = _desiredMin,
                    ToMax = _desiredMax,
                    Mean = sampleMean,
                    Std = sampleStd,
                };

                foreach (double[] input in inputs)
                {
                    input[i] = normPs[i].Normalize(input[i]);
                }
            }

            return normPs;
        }
    }
}

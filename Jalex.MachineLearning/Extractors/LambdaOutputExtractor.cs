using System;

namespace Jalex.MachineLearning.Extractors
{
    public class LambdaOutputExtractor<TInput, TOutput> : IOutputExtractor<TInput, TOutput>
    {
        private readonly Func<TInput, double[]> _outputExtractionFunc;
        private readonly Func<double[], IPrediction<TOutput>> _predictionCreationFunc;

        public LambdaOutputExtractor(Func<TInput, double[]> outputExtractionFunc, Func<double[], IPrediction<TOutput>> predictionCreationFunc)
        {
            if (outputExtractionFunc == null) throw new ArgumentNullException(nameof(outputExtractionFunc));
            if (predictionCreationFunc == null) throw new ArgumentNullException(nameof(predictionCreationFunc));

            _outputExtractionFunc = outputExtractionFunc;
            _predictionCreationFunc = predictionCreationFunc;
        }

        #region Implementation of IOutputExtractor

        public double[] ExtractOutputs(TInput input) => _outputExtractionFunc(input);

        public IPrediction<TOutput> CreatePrediction(double[] doubleOutputs) => _predictionCreationFunc(doubleOutputs);

        #endregion
    }
}

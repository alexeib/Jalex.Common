using System;

namespace Jalex.MachineLearning.Extractors
{
    public class LambdaPredictionCreator<TInput, TOutput> : IPredictionCreator<TInput, TOutput>
    {
        private readonly Func<TInput, double[], IPrediction<TInput, TOutput>> _predictionCreationFunc;

        public LambdaPredictionCreator(Func<TInput, double[], IPrediction<TInput, TOutput>> predictionCreationFunc)
        {
            if (predictionCreationFunc == null) throw new ArgumentNullException(nameof(predictionCreationFunc));

            _predictionCreationFunc = predictionCreationFunc;
        }

        #region Implementation of IOutputExtractor

        public IPrediction<TInput, TOutput> CreatePrediction(TInput input, double[] doubleOutputs) => _predictionCreationFunc(input, doubleOutputs);

        #endregion
    }
}

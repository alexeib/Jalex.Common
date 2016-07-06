using System;

namespace Jalex.MachineLearning.Extractors
{
    public class LambdaPredictionCreator<TOutput> : IPredictionCreator<TOutput>
    {
        private readonly Func<double[], IPrediction<TOutput>> _predictionCreationFunc;

        public LambdaPredictionCreator(Func<double[], IPrediction<TOutput>> predictionCreationFunc)
        {
            if (predictionCreationFunc == null) throw new ArgumentNullException(nameof(predictionCreationFunc));

            _predictionCreationFunc = predictionCreationFunc;
        }

        #region Implementation of IOutputExtractor

        public IPrediction<TOutput> CreatePrediction(double[] doubleOutputs) => _predictionCreationFunc(doubleOutputs);

        #endregion
    }
}

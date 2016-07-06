using System;
using Jalex.MachineLearning.Extractors;

namespace Jalex.MachineLearning.NaiveBayes
{
    public class NaiveBayesPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        private readonly Accord.MachineLearning.Bayes.NaiveBayes _bayes;
        private readonly IInputExtractor<TInput, int> _inputExtractor;
        private readonly IPredictionCreator<TOutput> _predictionCreator;

        public NaiveBayesPredictor(Accord.MachineLearning.Bayes.NaiveBayes bayes, IInputExtractor<TInput, int> inputExtractor, IPredictionCreator<TOutput> predictionCreator)
        {
            if (bayes == null) throw new ArgumentNullException(nameof(bayes));
            if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
            if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));

            _bayes = bayes;
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
        }

        #region Implementation of IPredictor<in TInput,out TOutput>

        public IPrediction<TOutput> ComputePrediction(TInput input)
        {
            var numericalInputs = _inputExtractor.ExtractInputs(input);
            if (numericalInputs == null) return null;
            double likelyhood;
            double[] responses;
            _bayes.Compute(numericalInputs, out likelyhood, out responses);
            return _predictionCreator.CreatePrediction(responses);
        }

        #endregion
    }
}

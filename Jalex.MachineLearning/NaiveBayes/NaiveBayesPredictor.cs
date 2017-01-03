using System;
using System.Collections.Generic;
using Jalex.MachineLearning.Extractors;

namespace Jalex.MachineLearning.NaiveBayes
{
    public class NaiveBayesPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        private readonly Accord.MachineLearning.Bayes.NaiveBayes _bayes;
        private readonly IInputExtractor<TInput, int> _inputExtractor;
        private readonly IPredictionCreator<TInput, TOutput> _predictionCreator;

	    public NaiveBayesPredictor(Accord.MachineLearning.Bayes.NaiveBayes bayes,
	                               IInputExtractor<TInput, int> inputExtractor,
	                               IPredictionCreator<TInput, TOutput> predictionCreator)
	    {
		    if (bayes == null) throw new ArgumentNullException(nameof(bayes));
		    if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
		    if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));

		    _bayes = bayes;
		    _inputExtractor = inputExtractor;
		    _predictionCreator = predictionCreator;
	    }

	    #region Implementation of IPredictor<in TInput,out TOutput>

        public IEnumerable<IPrediction<TInput, TOutput>> ComputePredictions(IEnumerable<TInput> inputs)
        {
	        foreach (var input in inputs)
	        {
		        var numericalInputs = _inputExtractor.ExtractInputs(input);
		        if (numericalInputs == null) yield return null;
		        else
		        {
			        double likelyhood;
			        double[] responses;
			        _bayes.Compute(numericalInputs, out likelyhood, out responses);
			        yield return _predictionCreator.CreatePrediction(input, responses);
		        }
	        }
        }

        #endregion
    }
}

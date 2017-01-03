using System;
using System.Collections.Generic;
using Accord.Neuro.Networks;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.DeepBelief
{
    public class DeepBeliefPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput, double> _inputExtractor;
        private readonly IPredictionCreator<TInput, TOutput> _predictionCreator;

        public DeepBeliefNetwork Network { get; }
        public NormalizationParams[] NormalizationParams { get; set; }

	    public DeepBeliefPredictor(DeepBeliefNetwork network,
	                               IInputExtractor<TInput, double> inputExtractor,
	                               IPredictionCreator<TInput, TOutput> predictionCreator,
	                               NormalizationParams[] normalizationParams)
	    {
		    if (normalizationParams == null) throw new ArgumentNullException(nameof(normalizationParams));
		    Network = network;
		    NormalizationParams = normalizationParams;
		    _inputExtractor = inputExtractor;
		    _predictionCreator = predictionCreator;
	    }

	    public IEnumerable<IPrediction<TInput, TOutput>> ComputePredictions(IEnumerable<TInput> inputs)
	    {
		    foreach (var input in inputs)
		    {
			    var numericInputs = _inputExtractor.ExtractInputs(input);
			    if (numericInputs == null)
			    {
				    yield return null;
			    }
			    else
			    {
				    normalize(numericInputs);
				    IPrediction<TInput, TOutput> prediction = null;
					try
				    {
					    var outputs = Network.Compute(numericInputs);
					    prediction = _predictionCreator.CreatePrediction(input, outputs);
				    }
				    catch (Exception e)
				    {
					    _logger.Error(e, $"Failed to generate prediction for input {input}");
				    }
					yield return prediction;
				}
			}
	    }

		private void normalize(double[] inputs)
		{
			if (NormalizationParams != null)
			{
				for (int i = 0; i < inputs.Length; i++)
				{
					inputs[i] = NormalizationParams[i].Normalize(inputs[i]);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Jalex.Infrastructure.Extensions;
using Jalex.MachineLearning.Extractors;

namespace Jalex.MachineLearning.Predicto
{
    public class PredictoPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
	    // ReSharper disable once ClassNeverInstantiated.Local
	    class PredictoResult
	    {
		    // ReSharper disable once UnusedAutoPropertyAccessor.Local
		    public double[][] Probabilities { get; set; }
	    }

        private readonly PredictoConfiguration _configuration;
        private readonly string _modelName;
        private readonly IInputExtractor<TInput, double> _inputExtractor;
        private readonly IPredictionCreator<TInput, TOutput> _predictionCreator;

	    public PredictoPredictor(IInputExtractor<TInput, double> inputExtractor,
	                             IPredictionCreator<TInput, TOutput> predictionCreator,
	                             PredictoConfiguration configuration,
	                             string modelName)
	    {
		    if (configuration == null) throw new ArgumentNullException(nameof(configuration));
		    if (modelName == null) throw new ArgumentNullException(nameof(modelName));
		    if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
		    if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));
		    _configuration = configuration;
		    _modelName = modelName;
		    _inputExtractor = inputExtractor;
		    _predictionCreator = predictionCreator;
	    }

	    public IEnumerable<IPrediction<TInput, TOutput>> ComputePredictions(IEnumerable<TInput> inputs)
	    {
			var baseUri = new Uri(_configuration.Endpoint);
			var modelUri = new Uri(baseUri, $"/predict/{_modelName}");

		    inputs = inputs.ToCollection();
		    var inputDoubles = inputs.Select(_inputExtractor.ExtractInputs)
		                             .ToList();

			using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
			{
				var result = client.PostAsJsonAsync(modelUri, inputDoubles)
									.Result.Content.ReadAsAsync<PredictoResult>()
									.Result;
				if (result?.Probabilities == null || result.Probabilities.Length == 0)
					throw new PredictionException($"Predicto prediction service at {modelUri} returned an empty result for model {_modelName}");
				return inputs.Zip(result.Probabilities, (i, r) => _predictionCreator.CreatePrediction(i, r));
			}
		}
    }
}

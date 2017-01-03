using System;
using System.Collections.Generic;
using System.Net.Http;
using Jalex.Infrastructure.Extensions;
using Jalex.MachineLearning.Extractors;

namespace Jalex.MachineLearning.Predicto
{
	public class PredictoTrainer<TInput, TOutput> : ITrainer<TInput, TOutput>
	{
		private readonly IInputExtractor<TInput, double> _inputExtractor;
		private readonly IPredictionCreator<TInput, TOutput> _predictionCreator;
		private readonly PredictoConfiguration _configuration;

		public PredictoTrainer(IInputExtractor<TInput, double> inputExtractor,
		                       IPredictionCreator<TInput, TOutput> predictionCreator,
		                       PredictoConfiguration configuration)
		{
			if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
			if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			_inputExtractor = inputExtractor;
			_predictionCreator = predictionCreator;
			_configuration = configuration;
		}

		public IPredictor<TInput, TOutput> Train(IEnumerable<Tuple<TInput, double[]>> inputsAndOutputs)
		{
			var baseUri = new Uri(_configuration.Endpoint);
			var trainUri = new Uri(baseUri, "/train");

			var inputDoubles = new List<double[]>();
			var outputs = new List<double>();

			foreach (var entry in inputsAndOutputs)
			{
				if (entry.Item2.Length != 1) throw new NotSupportedException("Predicto trainer currently only supports exactly 1 output, but got " + entry.Item2.Length);
				var inputs = _inputExtractor.ExtractInputs(entry.Item1);
				inputDoubles.Add(inputs);
				outputs.Add(entry.Item2[0]);
			}

			using (HttpClient client = new HttpClient {Timeout = TimeSpan.FromDays(1) })
			{
				var modelId = client.PostAsJsonAsync(trainUri, new { data = new { inputs = inputDoubles, outputs } })
									.Result.Content.ReadAsStringAsync()
									.Result;
				if (modelId.IsNullOrEmpty()) return null;
				var predictor = new PredictoPredictor<TInput, TOutput>(_inputExtractor, _predictionCreator, _configuration, modelId);
				return predictor;
			}
		}
	}
}

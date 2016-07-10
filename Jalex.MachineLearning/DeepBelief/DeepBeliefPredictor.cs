using System;
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
        private readonly IPredictionCreator<TOutput> _predictionCreator;

        public DeepBeliefNetwork Network { get; }
        public NormalizationParams[] NormalizationParams { get; set; }

        public DeepBeliefPredictor(DeepBeliefNetwork network, IInputExtractor<TInput, double> inputExtractor, IPredictionCreator<TOutput> predictionCreator, NormalizationParams[] normalizationParams)
        {
            if (normalizationParams == null) throw new ArgumentNullException(nameof(normalizationParams));
            Network = network;
            NormalizationParams = normalizationParams;
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
        }

        #region Implementation of IPredictor

        public IPrediction<TOutput> ComputePrediction(TInput input)
        {
            var inputs = _inputExtractor.ExtractInputs(input);
            if (inputs == null)
            {
                return null;
            }

            normalize(inputs);

            try
            {
                var outputs = Network.Compute(inputs);
                var prediction = _predictionCreator.CreatePrediction(outputs);
                return prediction;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to generate prediction for input {input}");
                return null;
            }
        }

        #endregion

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

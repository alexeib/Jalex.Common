using System;
using Accord.MachineLearning.VectorMachines;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.SVM
{
    public class DtwSvmPredictor<TInput, TOutput> : IPredictor<TInput,TOutput>
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput, double> _inputExtractor;
        private readonly IPredictionCreator<TOutput> _predictionCreator;

        public NormalizationParams[] NormalzationParams { get; }

        public ISupportVectorMachine[] Svms { get; }

        public DtwSvmPredictor(ISupportVectorMachine[] svms, IInputExtractor<TInput, double> inputExtractor, IPredictionCreator<TOutput> predictionCreator, NormalizationParams[] normalzationParams)
        {
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
            Svms = svms;
            NormalzationParams = normalzationParams;
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
                var outputs = new double[Svms.Length];

                for (int i = 0; i < Svms.Length; i++)
                {
                    outputs[i] = getOutput(inputs, Svms[i]);
                }

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
            if (NormalzationParams == null) return;

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = NormalzationParams[i].Normalize(inputs[i]);
            }
        }

        private double getOutput(double[] inputs, ISupportVectorMachine supportVectorMachine)
        {
            double probability;
            supportVectorMachine.Compute(inputs, out probability);
            return probability;
        }
    }
}
using System;
using Accord.MachineLearning.VectorMachines;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.SVM
{
    public class DtwSvmPredictor<TInput, TOutput> : IPredictor<TInput,TOutput>
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput> _inputExtractor;
        private readonly IOutputExtractor<TInput, TOutput> _outputExtractor;

        public Tuple<double, double>[] InputMeanAndStd { get; }

        public ISupportVectorMachine[] Svms { get; }

        public DtwSvmPredictor(ISupportVectorMachine[] svms, IInputExtractor<TInput> inputExtractor, IOutputExtractor<TInput, TOutput> outputExtractor, Tuple<double, double>[] inputMeanAndStd)
        {
            _inputExtractor = inputExtractor;
            _outputExtractor = outputExtractor;
            Svms = svms;
            InputMeanAndStd = inputMeanAndStd;
        }

        #region Implementation of IPredictor

        public IPrediction<TOutput> ComputePrediction(TInput input)
        {
            var inputs = _inputExtractor.ExtractInputs(input);
            if (inputs == null)
            {
                return null;
            }

            normalizeInputs(inputs);

            try
            {
                var outputs = new double[Svms.Length];

                for (int i = 0; i < Svms.Length; i++)
                {
                    outputs[i] = getOutput(inputs, Svms[i]);
                }

                var prediction = _outputExtractor.CreatePrediction(outputs);
                return prediction;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to generate prediction for input {input}");
                return null;
            }
        }

        #endregion

        private void normalizeInputs(double[] inputs)
        {
            if (InputMeanAndStd == null) return;

            for (int i = 0; i < inputs.Length; i++)
            {
                if (!double.IsNaN(InputMeanAndStd[i].Item2))
                {
                    inputs[i] = (inputs[i] - InputMeanAndStd[i].Item1) / InputMeanAndStd[i].Item2;
                }
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
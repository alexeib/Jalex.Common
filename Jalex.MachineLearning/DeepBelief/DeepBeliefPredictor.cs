﻿using System;
using Accord.Neuro.Networks;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.DeepBelief
{
    public class DeepBeliefPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput> _inputExtractor;
        private readonly IOutputExtractor<TInput, TOutput> _outputExtractor;

        public Tuple<double, double>[] InputMeanAndStd { get; }

        public DeepBeliefNetwork Network { get; }

        public DeepBeliefPredictor(DeepBeliefNetwork network, IInputExtractor<TInput> inputExtractor, IOutputExtractor<TInput, TOutput> outputExtractor, Tuple<double, double>[] inputMeanAndStd)
        {
            Network = network;
            InputMeanAndStd = inputMeanAndStd;
            _inputExtractor = inputExtractor;
            _outputExtractor = outputExtractor;
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
                var outputs = Network.Compute(inputs);
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
    }
}

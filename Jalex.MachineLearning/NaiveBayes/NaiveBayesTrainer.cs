using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Extensions;
using Jalex.MachineLearning.Extractors;

namespace Jalex.MachineLearning.NaiveBayes
{
    /// <summary>
    ///  for now only support 2 classes per symbol (i.e. yes/no)
    /// </summary>
    public class NaiveBayesTrainer<TInput, TOutput> : ITrainer<TInput, TOutput>
    {
        private readonly IInputExtractor<TInput, int> _inputExtractor;
        private readonly IPredictionCreator<TOutput> _predictionCreator;
        private readonly InputBuilder<TInput, int> _inputBuilder;

        public NaiveBayesTrainer(IInputExtractor<TInput, int> inputExtractor, IPredictionCreator<TOutput> predictionCreator)
        {
            if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
            if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;

            _inputBuilder = new InputBuilder<TInput, int>(_inputExtractor);
        }

        #region Implementation of ITrainer<TInput,out TOutput>

        public IPredictor<TInput, TOutput> Train(IEnumerable<Tuple<TInput, double[]>> inputsAndOutputs)
        {
            var inpOutColl = inputsAndOutputs.ToCollection();

            int[] numericalOutputs = inpOutColl.Select(x => (int) Math.Round(x.Item2[0]))
                                               .ToArray();
            var numericalInputs = _inputBuilder.BuildInputs(inpOutColl.Select(x => x.Item1));

            if (numericalInputs.Length == 0)
            {
                return null;
            }

            if (inpOutColl.First().Item2.Length != 1)
            {
                throw new InvalidOperationException("Current implementation of Naive Bayes only supports a single class");
            }

            var symbols = Enumerable.Range(0, numericalInputs[0].Length)
                                    .Select(_ => 2)
                                    .ToArray();

            var bayes = new Accord.MachineLearning.Bayes.NaiveBayes(2, symbols);
            bayes.Estimate(numericalInputs, numericalOutputs);

            for (int c = 0; c < 2; c++)
            {
                for (int i = 0; i < numericalInputs[0].Length; i++)
                {
                    if (bayes.Distributions[c, i][0] == 0 || bayes.Distributions[c, i][0] == 1)
                    {
                        throw new Exception();
                    }
                }
            }

            return new NaiveBayesPredictor<TInput, TOutput>(bayes, _inputExtractor, _predictionCreator);
        }

        #endregion
    }
}

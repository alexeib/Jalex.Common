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
        private readonly int _outputClasses;
        private readonly int[] _inputClasses;
        private readonly InputBuilder<TInput, int> _inputBuilder;

        public NaiveBayesTrainer(IInputExtractor<TInput, int> inputExtractor, IPredictionCreator<TOutput> predictionCreator, int outputClasses, IEnumerable<int> inputClasses)
        {
            if (inputExtractor == null) throw new ArgumentNullException(nameof(inputExtractor));
            if (predictionCreator == null) throw new ArgumentNullException(nameof(predictionCreator));
            if (inputClasses == null) throw new ArgumentNullException(nameof(inputClasses));

            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
            _outputClasses = outputClasses;
            _inputClasses = inputClasses.ToArray();

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

            var bayes = new Accord.MachineLearning.Bayes.NaiveBayes(_outputClasses, _inputClasses);
            bayes.Estimate(numericalInputs, numericalOutputs);

            for (int c = 0; c < _outputClasses; c++)
            {
                for (int i = 0; i < _inputClasses.Length; i++)
                {
                    var curr = bayes.Distributions[c, i];
                    var sum = curr.Sum();
                    if (sum < 1)
                    {
                        var toAdd = (1 - sum)/curr.Length;
                        for (int j = 0; j < curr.Length; j++)
                        {
                            curr[j] += toAdd;
                        }
                    }
                }
            }

            return new NaiveBayesPredictor<TInput, TOutput>(bayes, _inputExtractor, _predictionCreator);
        }

        #endregion
    }
}

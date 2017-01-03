using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using AForge.Neuro.Learning;
using Jalex.MachineLearning.Extractors;
using NLog;
using Jalex.Infrastructure.Extensions;

namespace Jalex.MachineLearning.DeepBelief
{
    public class DeepBeliefTrainer<TInput, TOutput> : ITrainer<TInput, TOutput>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput, double> _inputExtractor;
        private readonly IPredictionCreator<TInput, TOutput> _predictionCreator;
        private readonly DeepBeliefNetworkSettings _settings;
        private readonly DeepBeliefNetwork _network;
        private readonly DoubleInputBuilder<TInput> _doubleInputBuilder;

        public DeepBeliefTrainer(IInputExtractor<TInput, double> inputExtractor,
                                 IPredictionCreator<TInput, TOutput> predictionCreator,
                                 DeepBeliefNetworkSettings settings,
                                 DeepBeliefNetwork network = null)
        {
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
            _settings = settings;
            _network = network;

            _doubleInputBuilder = new DoubleInputBuilder<TInput>(inputExtractor);
        }

        public bool IsLoggingEnabled { get; set; }

        #region Implementation of INeuralTrainer

        public IPredictor<TInput, TOutput> Train(IEnumerable<Tuple<TInput, double[]>> inputsAndOutputs)
        {
            var inpOutColl = inputsAndOutputs.ToCollection();
            double[][] numericalOutputs = inpOutColl.Select(x => x.Item2)
                                                    .ToArray();
            var numericalInputs = _doubleInputBuilder.BuildInputs(inpOutColl.Select(x => x.Item1));

            if (numericalInputs.Length == 0)
            {
                return null;
            }

            var normalizationParams = _doubleInputBuilder.NormalizeInputs(numericalInputs);

            var network = _network ?? CreateNetwork(numericalInputs[0].Length, numericalOutputs[0].Length, _settings);

            trainHiddenLayers(network, numericalInputs);
            trainNetwork(network, numericalInputs, numericalOutputs);

            return new DeepBeliefPredictor<TInput, TOutput>(network, _inputExtractor, _predictionCreator, normalizationParams);
        }

        public static DeepBeliefNetwork CreateNetwork(int inputLength, int outputLength, DeepBeliefNetworkSettings settings)
        {
            var hiddenLayerNeurons = settings.Layers.Select(l => l.Neurons)
                                                .Concat(outputLength.ToEnumerable())
                                                .ToArray();

            var network = new DeepBeliefNetwork(inputLength, hiddenLayerNeurons);

            for (int i = 0; i < settings.Layers.Count; i++)
            {
                foreach (var neuron in network.Machines[i].Visible.Neurons)
                {
                    neuron.ActivationFunction = settings.Layers[i].GetVisibleActivationFunction();
                }

                foreach (var neuron in network.Machines[i].Hidden.Neurons)
                {
                    neuron.ActivationFunction = settings.Layers[i].GetHiddenActivationFunction();
                }
            }

            switch (settings.InitializationType)
            {
                case WeightInitializationType.NguyenWidrow:
                    new NguyenWidrow(network).Randomize();
                    network.UpdateVisibleWeights();
                    break;
                case WeightInitializationType.Gaussian:
                    new GaussianWeights(network).Randomize();
                    network.UpdateVisibleWeights();
                    break;
                case WeightInitializationType.Random:
                    break;
                default:
                    throw new IndexOutOfRangeException("Unknown initialization type " + settings.InitializationType);
            }

            return network;
        }

        #endregion

        private void trainNetwork(DeepBeliefNetwork network, double[][] inputs, double[][] outputs)
        {
            var teacher = new BackPropagationLearning(network)
            {
                LearningRate = _settings.LearningRate,
                Momentum = _settings.Momentum
            };

            var random = Tools.Random;

            for (int x = 0; x < _settings.NumIterations; x++)
            {
                var zipped = inputs.Zip(outputs, (i, o) => Tuple.Create(i, o, random.Next()));
                var shuffled = zipped.OrderBy(tpl => tpl.Item3);

                var innerInputs = new List<double[]>();
                var innerOutputs = new List<double[]>();

                foreach (var pair in shuffled)
                {
                    innerInputs.Add(pair.Item1);
                    innerOutputs.Add(pair.Item2);
                }

                for (int i = 0; i < _settings.Epochs; i++)
                {
                    var error = teacher.RunEpoch(innerInputs.ToArray(), innerOutputs.ToArray()) / inputs.Length;
                    if (IsLoggingEnabled && i % 100 == 0)
                    {
                        _logger.Info($"Backpropagation, Epoch {i}, Error = {error}");
                    }
                }
            }
        }

        private void trainHiddenLayers(DeepBeliefNetwork network, double[][] inputs)
        {
            for (int x = 0; x < _settings.NumIterations; x++)
            {
                var batchCount = Math.Max(1, inputs.Length / _settings.BatchSize);
                var groups = categoricalRandom(inputs.Length, batchCount);
                var batches = inputs.Subgroups(groups);

                for (int layerIndex = 0; layerIndex < _settings.Layers.Count; layerIndex++)
                {
                    var hiddenLayerParameters = _settings.Layers.ElementAt(layerIndex);
                    var teacher = new DeepBeliefNetworkLearning(network)
                    {
                        Algorithm = (h, v, i) => new ContrastiveDivergenceLearning(h, v)
                        {
                            LearningRate = hiddenLayerParameters.LearningRate,
                            Momentum = hiddenLayerParameters.Momentum,
                            Decay = hiddenLayerParameters.Decay
                        },
                        LayerIndex = layerIndex
                    };

                    var layerData = teacher.GetLayerInput(batches);
                    for (int i = 0; i < hiddenLayerParameters.Epochs; i++)
                    {
                        var error = teacher.RunEpoch(layerData) / inputs.Length;
                        if (IsLoggingEnabled && i % 100 == 0)
                        {
                            _logger.Info($"Hidden layer {layerIndex}, Epoch {i}, Error = {error}");
                        }
                    }
                }
            }
        }

        // below is copied from Accord code because of difference in versions 

        /// <summary>
        ///   Returns a random group assignment for a sample.
        /// </summary>
        /// 
        /// <param name="samples">The sample size.</param>
        /// <param name="categories">The number of groups.</param>
        /// 
        private static int[] categoricalRandom(int samples, int categories)
        {
            // Create the index vector
            int[] idx = new int[samples];

            if (categories == 1)
                return idx;

            double n = categories / (double)samples;
            for (int i = 0; i < idx.Length; i++)
                idx[i] = (int)Math.Ceiling((i + 0.9) * n) - 1;

            // Shuffle the indices vector
            shuffle(idx);

            return idx;
        }

        /// <summary>
        ///   Shuffles an array.
        /// </summary>
        /// 
        private static void shuffle<T>(T[] array)
        {
            var random = Tools.Random;

            // i is the number of items remaining to be shuffled.
            for (int i = array.Length; i > 1; i--)
            {
                // Pick a random element to swap with the i-th element.
                int j = random.Next(i);

                // Swap array elements.
                var aux = array[j];
                array[j] = array[i - 1];
                array[i - 1] = aux;
            }
        }
    }
}

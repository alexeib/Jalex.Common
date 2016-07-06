using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Jalex.Infrastructure.Extensions;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.SVM
{
    public class DtwSvmTrainer<TInput, TOutput> : ITrainer<TInput,TOutput>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput, double> _inputExtractor;
        private readonly IPredictionCreator<TOutput> _predictionCreator;
        private readonly SvmSettings _settings;
        private readonly DoubleInputBuilder<TInput> _doubleInputBuilder;

        public bool IsLoggingEnabled { get; set; } = true;

        public DtwSvmTrainer(IInputExtractor<TInput, double> inputExtractor, IPredictionCreator<TOutput> predictionCreator, SvmSettings settings)
        {
            _inputExtractor = inputExtractor;
            _predictionCreator = predictionCreator;
            _settings = settings;

            _doubleInputBuilder = new DoubleInputBuilder<TInput>(inputExtractor);
        }

        #region Implementation of ITrainer

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

            var outputLength = numericalOutputs[0].Length;
            var meanStd = _doubleInputBuilder.NormalizeInputs(numericalInputs);

            ISupportVectorMachine[] svms = new ISupportVectorMachine[outputLength];

            Enumerable.Range(0, outputLength)
                      .OrderBy(i => Math.Abs(i - outputLength / 2)) // start from the middle as it is most computationally intensive
                      .AsParallel()
                      .ForAll(i => svms[i] = trainSvm(numericalInputs, numericalOutputs, i));

            if (IsLoggingEnabled)
            {
                _logger.Info("Training finished");
            }

            return new DtwSvmPredictor<TInput, TOutput>(svms, _inputExtractor, _predictionCreator, meanStd);
        }

        #endregion

        private ISupportVectorMachine trainSvm(double[][] inputs, double[][] outputs, int outputIndex)
        {
            var kernel = new DynamicTimeWarping(inputs[0].Length, _settings.Alpha, _settings.Degree);
            var svm = new KernelSupportVectorMachine(kernel, inputs[0].Length);
            int[] outputInts = extractOutput(outputs, outputIndex);

            ISupportVectorMachineLearning svmLearning;

            //switch (_settings.LearningType)
            //{
            //    case SvmLearningType.ProbabilisticDualCoordinateDescent:
            //        svmLearning = new ProbabilisticDualCoordinateDescent(svm, inputs, outputInts)
            //                      {
            //                          MaximumIterations = _settings.MaximumIterations,
            //                          MaximumNewtonIterations = _settings.MaximumNewtonIterations,
            //                          Tolerance = _settings.Tolerance,
            //                          Complexity = _settings.Complexity,
            //                          UseComplexityHeuristic = _settings.UseComplexityHeuristic
            //                      };
            //        break;
            //    case SvmLearningType.ProbabilisticCoordinateDescent:
            //        svmLearning = new ProbabilisticCoordinateDescent(svm, inputs, outputInts)
            //        {
            //            MaximumIterations = _settings.MaximumIterations,
            //            MaximumNewtonIterations = _settings.MaximumNewtonIterations,
            //            Tolerance = _settings.Tolerance,
            //            Complexity = _settings.Complexity,
            //            UseComplexityHeuristic = _settings.UseComplexityHeuristic
            //        };
            //        break;
            //    case SvmLearningType.ProbabilisticNewtonMethod:
            //        svmLearning = new ProbabilisticNewtonMethod(svm, inputs, outputInts)
            //        {
            //            MaximumIterations = _settings.MaximumIterations,
            //            Tolerance = _settings.Tolerance,
            //            Complexity = _settings.Complexity,
            //            UseComplexityHeuristic = _settings.UseComplexityHeuristic
            //        };
            //        break;
            //    default:
            //        throw new IndexOutOfRangeException($"Unknown svm learning type {_settings.LearningType}");
            //}

            if (IsLoggingEnabled)
            {
                _logger.Info($"Starting training output at index {outputIndex}");
            }

            svmLearning = new SequentialMinimalOptimization(svm, inputs, outputInts)
            {
                Complexity = _settings.Complexity,
                UseComplexityHeuristic = _settings.UseComplexityHeuristic,
                Tolerance = _settings.Tolerance,
                CacheSize = _settings.CacheSize
            };

            var optError = svmLearning.Run();

            if (IsLoggingEnabled)
            {
                _logger.Info($"Finished optimization at index {outputIndex} with error {optError:N6}");
            }

            var calibrator = new ProbabilisticOutputCalibration(svm, inputs, outputInts)
            {
                Iterations = _settings.CalibrationIterations,
                Tolerance = _settings.CalibrationTolerance,
                StepSize = _settings.CalibrationStepSize
            };

            var calibrationError = calibrator.Run();

            if (IsLoggingEnabled)
            {
                _logger.Info($"Finished calibration at index {outputIndex} with error {calibrationError:N6}");
            }

            return svm;
        }

        private int[] extractOutput(double[][] outputs, int outputIndex)
        {
            int[] result = new int[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                var val = outputs[i][outputIndex];
                if (val < 0 || val > 1) throw new InvalidOperationException($"SVM output must be between 0 and 1 (inclusive) but is {val}");

                if (val >= 0.8) result[i] = 1;
                else result[i] = -1;
            }

            return result;
        }
    }
}

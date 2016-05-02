using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Jalex.MachineLearning.Extractors;
using NLog;

namespace Jalex.MachineLearning.SVM
{
    public class DtwSvmTrainer<TInput, TOutput> : ITrainer<TInput,TOutput>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IInputExtractor<TInput> _inputExtractor;
        private readonly IOutputExtractor<TInput, TOutput> _outputExtractor;
        private readonly SvmSettings _settings;
        private readonly InputOutputBuilder<TInput, TOutput> _inputOutputBuilder;

        public bool IsLoggingEnabled { get; set; } = true;

        public DtwSvmTrainer(IInputExtractor<TInput> inputExtractor, IOutputExtractor<TInput, TOutput> outputExtractor, SvmSettings settings)
        {
            _inputExtractor = inputExtractor;
            _outputExtractor = outputExtractor;
            _settings = settings;

            _inputOutputBuilder = new InputOutputBuilder<TInput, TOutput>(inputExtractor, outputExtractor);
        }

        #region Implementation of ITrainer

        public IPredictor<TInput, TOutput> Train(IEnumerable<TInput> inputs)
        {
            double[][] numericalInputs, outputs;
            _inputOutputBuilder.BuildInputOutputs(inputs, out numericalInputs, out outputs);

            if (numericalInputs.Length == 0)
            {
                return null;
            }

            var outputLength = outputs[0].Length;

            var meanStd = _inputOutputBuilder.NormalizeInputs(numericalInputs);

            ISupportVectorMachine[] svms = new ISupportVectorMachine[outputLength];

            Enumerable.Range(0, outputLength)
                      .OrderBy(i => Math.Abs(i - outputLength / 2)) // start from the middle as it is most computationally intensive
                      .AsParallel()
                      .ForAll(i => svms[i] = trainSvm(numericalInputs, outputs, i));

            if (IsLoggingEnabled)
            {
                _logger.Info("Training finished");
            }

            return new DtwSvmPredictor<TInput, TOutput>(svms, _inputExtractor, _outputExtractor, meanStd);
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

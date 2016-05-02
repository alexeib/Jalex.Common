using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics;

namespace Jalex.MachineLearning.Extractors
{
    internal class InputOutputBuilder<TInput, TOutput>
    {
        private readonly IInputExtractor<TInput> _inputExtractor;
        private readonly IOutputExtractor<TInput, TOutput> _outputExtractor;

        public InputOutputBuilder(IInputExtractor<TInput> inputExtractor, IOutputExtractor<TInput, TOutput> outputExtractor)
        {
            _inputExtractor = inputExtractor;
            _outputExtractor = outputExtractor;
        }

        public void BuildInputOutputs(IEnumerable<TInput> inputs, out double[][] inputsArr, out double[][] outputsArr)
        {
            var pairs = inputOutputPairs(inputs);
            List<double[]> numericalInputs = new List<double[]>();
            List<double[]> outputs = new List<double[]>();

            foreach (var pair in pairs)
            {
                numericalInputs.Add(pair.Item1);
                outputs.Add(pair.Item2);
            }

            inputsArr = numericalInputs.ToArray();
            outputsArr = outputs.ToArray();
        }

        public Tuple<double, double>[] NormalizeInputs(double[][] inputs)
        {
            return null;

            //var meanStd = new Tuple<double, double>[inputs[0].Length];

            //var numParameters = inputs[0].Length;
            //for (int i = 0; i < numParameters; i++)
            //{
            //    var samples = inputs.Select(x => x[i])
            //                        .ToArray();
            //    var mean = samples.Mean();
            //    var std = samples.StandardDeviation(mean);

            //    if (!double.IsNaN(std))
            //    {
            //        foreach (double[] input in inputs)
            //        {
            //            input[i] = (input[i] - mean)/std;
            //        }
            //    }

            //    meanStd[i] = new Tuple<double, double>(mean, std);
            //}

            //return meanStd;
        }

        private IEnumerable<Tuple<double[], double[]>> inputOutputPairs(IEnumerable<TInput> inputs)
        {
            return from input in inputs
                   let numericalInputs = _inputExtractor.ExtractInputs(input)
                   let outputs = _outputExtractor.ExtractOutputs(input)
                   where inputs != null && outputs != null
                   select new Tuple<double[], double[]>(numericalInputs, outputs);
        }
    }
}

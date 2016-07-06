using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning
{
    public interface ITrainer<TInput, out TOutput>
    {
        IPredictor<TInput,TOutput> Train(IEnumerable<Tuple<TInput, double[]>> inputsAndOutputs);
    }
}
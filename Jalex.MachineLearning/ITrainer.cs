using System.Collections.Generic;

namespace Jalex.MachineLearning
{
    public interface ITrainer<in TInput, out TOutput>
    {
        IPredictor<TInput,TOutput> Train(IEnumerable<TInput> inputs);
    }
}
namespace Jalex.MachineLearning.Extractors
{
    public interface IPredictionCreator<TInput, out TOutput>
    {
        /// <summary>
        /// Given a set of outputs in the same format as produced by ExtractOutputs, produces a computed result object
        /// </summary>
        IPrediction<TInput, TOutput> CreatePrediction(TInput input, double [] doubleOutputs);
    }
}
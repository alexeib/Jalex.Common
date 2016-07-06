namespace Jalex.MachineLearning.Extractors
{
    public interface IPredictionCreator<out TOutput>
    {
        /// <summary>
        /// Given a set of outputs in the same format as produced by ExtractOutputs, produces a computed result object
        /// </summary>
        IPrediction<TOutput> CreatePrediction(double[] doubleOutputs);
    }
}
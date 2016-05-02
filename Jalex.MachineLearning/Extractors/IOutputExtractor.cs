namespace Jalex.MachineLearning.Extractors
{
    public interface IOutputExtractor<in TInput, out TOutput>
    {
        /// <summary>
        /// Extracts a normalized vector of classifications to be used as ouputs for a machine learning from a given stock for a given input
        /// </summary>
        double[] ExtractOutputs(TInput input);

        /// <summary>
        /// Given a set of outputs in the same format as produced by ExtractOutputs, produces a computed result object
        /// </summary>
        IPrediction<TOutput> CreatePrediction(double[] doubleOutputs);
    }
}
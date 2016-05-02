namespace Jalex.MachineLearning.Extractors
{
    public interface IInputExtractor<in TInput>
    {
        /// <summary>
        /// Extracts a normalized vector of "features" that are to be used as inputs for a machine learning from a given input
        /// </summary>
        double[] ExtractInputs(TInput input);
    }
}

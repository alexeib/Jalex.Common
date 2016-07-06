namespace Jalex.MachineLearning.Extractors
{
    public interface IInputExtractor<in TInstance, out TInput>
    {
        /// <summary>
        /// Extracts a normalized vector of "features" that are to be used as inputs for a machine learning from a given input
        /// </summary>
        TInput[] ExtractInputs(TInstance input);
    }
}

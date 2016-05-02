namespace Jalex.MachineLearning
{
    public interface IPredictor<in TInput, out TOutput>
    {
        IPrediction<TOutput> ComputePrediction(TInput input);
    }
}
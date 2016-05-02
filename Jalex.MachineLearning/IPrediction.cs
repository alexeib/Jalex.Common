namespace Jalex.MachineLearning
{
    public interface IPrediction<out TOutput>
    {
         TOutput Value { get; }
    }
}
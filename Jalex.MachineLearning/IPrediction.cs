namespace Jalex.MachineLearning
{
	public interface IPrediction<out TInput, out TOutput>
	{
		TInput Input { get; }
		TOutput Value { get; }
	}
}
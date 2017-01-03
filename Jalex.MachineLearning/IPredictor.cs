using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Extensions;

namespace Jalex.MachineLearning
{
    public interface IPredictor<TInput, out TOutput>
    {
        IEnumerable<IPrediction<TInput, TOutput>> ComputePredictions(IEnumerable<TInput> inputs);
    }

	public static class PredictorExtensions
	{
		public static IPrediction<TInput, TOutput> ComputePrediction<TInput, TOutput>(this IPredictor<TInput, TOutput> predictor, TInput input)
		{
			return predictor.ComputePredictions(input.ToEnumerable())
			                .Single();
		}
	}
}
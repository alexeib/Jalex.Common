using System;
using System.Collections.Generic;
using System.Linq;

namespace Jalex.MachineLearning
{
    public class LambdaPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        private readonly Func<IEnumerable<TInput>, IEnumerable<IPrediction<TInput, TOutput>>> _predictionFunc;

        public LambdaPredictor(Func<TInput, IPrediction<TInput, TOutput>> predictionFunc)
        {
            if (predictionFunc == null) throw new ArgumentNullException(nameof(predictionFunc));
            _predictionFunc = inps => inps.Select(predictionFunc);
        }

		public LambdaPredictor(Func<IEnumerable<TInput>, IEnumerable<IPrediction<TInput, TOutput>>> predictionFunc)
		{
			if (predictionFunc == null) throw new ArgumentNullException(nameof(predictionFunc));
			_predictionFunc = predictionFunc;
		}

		#region Implementation of IPredictor<in TInput,out TOutput>

		public IEnumerable<IPrediction<TInput, TOutput>> ComputePredictions(IEnumerable<TInput> input)
        {
            return _predictionFunc(input);
        }

        #endregion
    }
}

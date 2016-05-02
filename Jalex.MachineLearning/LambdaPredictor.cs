using System;

namespace Jalex.MachineLearning
{
    public class LambdaPredictor<TInput, TOutput> : IPredictor<TInput, TOutput>
    {
        private readonly Func<TInput, IPrediction<TOutput>> _predictionFunc;

        public LambdaPredictor(Func<TInput, IPrediction<TOutput>> predictionFunc)
        {
            if (predictionFunc == null) throw new ArgumentNullException(nameof(predictionFunc));
            _predictionFunc = predictionFunc;
        }

        #region Implementation of IPredictor<in TInput,out TOutput>

        public IPrediction<TOutput> ComputePrediction(TInput input)
        {
            return _predictionFunc(input);
        }

        #endregion
    }
}

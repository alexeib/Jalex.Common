namespace Jalex.MachineLearning.Concrete
{
    public class ValuePrediction<TInput, TOutput> : IPrediction<TInput, TOutput>
    {
		public TInput Input { get; }
        public TOutput Value { get; }

        public ValuePrediction(TInput input, TOutput value)
        {
	        Input = input;
	        Value = value;
        }
    }
}

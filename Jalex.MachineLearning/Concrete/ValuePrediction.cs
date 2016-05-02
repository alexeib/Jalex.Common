namespace Jalex.MachineLearning.Concrete
{
    public class ValuePrediction<T> : IPrediction<T>
    {
        public T Value { get; set; }

        public ValuePrediction(T value)
        {
            Value = value;
        }
    }
}

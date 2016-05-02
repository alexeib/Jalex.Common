namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class Range<T>
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }

        public Range(T minimum, T maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
    }
}

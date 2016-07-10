namespace Jalex.MachineLearning.Extractors
{
    public class NormalizationParams
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double ScaledMean { get; set; }
        public double ScaledStd { get; set; }

        public double Normalize(double v)
        {
            var d = Max - Min;
            return (v - Min - d * ScaledMean) / (d * ScaledStd); // scale to [-1,1] then center mean
        }
    }
}

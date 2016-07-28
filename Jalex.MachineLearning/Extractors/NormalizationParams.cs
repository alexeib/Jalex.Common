using System.Diagnostics;

namespace Jalex.MachineLearning.Extractors
{
    public class NormalizationParams
    {
        public double FromMin { get; set; }
        public double FromMax { get; set; }
        public double ToMin { get; set; }
        public double ToMax { get; set; }
        public double Mean { get; set; }
        public double Std { get; set; }

        public double Normalize(double v)
        {            
            var centered = Standardize(v, Mean, Std); // center mean
            return Scale(centered);
        }

        public static double Standardize(double v, double mean, double std)
        {
            return v;
            //return (v - mean)/std;
        }

        public double Scale(double v)
        {
            var x = (v - FromMin)*(ToMax - ToMin)/(FromMax - FromMin) + ToMin;
            return x;
        }
    }
}

using System.Text;
using Jalex.MachineLearning.Tuning;

namespace Jalex.MachineLearning.SVM
{
    public class SvmSettings : ITrainerSettings
    {
        [TunableParameter]
        public double Alpha { get; set; } = 1;
        [TunableParameter]
        public int Degree { get; set; } = 1;
        //public SvmLearningType LearningType { get; set; } = SvmLearningType.ProbabilisticDualCoordinateDescent;
        //public int MaximumIterations { get; set; } = 1000;
        //public int MaximumNewtonIterations { get; set; } = 100;
        [TunableParameter]
        public double Tolerance { get; set; } = 0.025; //0.1;
        [TunableParameter]
        public double Complexity { get; set; } = 1.5;
        [TunableParameter]
        public bool UseComplexityHeuristic { get; set; } = true;
        [TunableParameter]
        public int CalibrationIterations { get; set; } = 100;
        [TunableParameter]
        public double CalibrationTolerance { get; set; } = 2.5E-06;
        [TunableParameter]
        public double CalibrationStepSize { get; set; } = 1e-10;

        public int CacheSize { get; set; } = 4000;

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Alpha: {Alpha}");
            sb.AppendLine($"Degree: {Degree}");
            sb.AppendLine($"Tolerance: {Tolerance}");
            sb.AppendLine($"Complexity: {Complexity}");
            sb.AppendLine($"Use Complexity Heuristic: {UseComplexityHeuristic}");
            sb.AppendLine($"Calibration Iterations: {CalibrationIterations}");
            sb.AppendLine($"Calibration Tolerance: {CalibrationTolerance}");
            sb.AppendLine($"Calibration Step Size: {CalibrationStepSize}");
            sb.AppendLine();

            return sb.ToString();
        }

        #endregion
    }
}

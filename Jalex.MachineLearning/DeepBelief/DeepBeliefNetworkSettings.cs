using System.Collections.Generic;
using System.Text;
using Jalex.MachineLearning.Tuning;

namespace Jalex.MachineLearning.DeepBelief
{
    public class DeepBeliefNetworkSettings : ITrainerSettings
    {
        [TunableParameter]
        public int NumIterations { get; set; } = 1;

        [TunableParameter]
        public IList<LayerParameters> Layers { get; set; } = new[]
                                                             {
                                                                 new LayerParameters
                                                                 {
                                                                     VisibleLayerType = LayerType.Gaussian,
                                                                     HiddenLayerType = LayerType.Gaussian,
                                                                     Neurons = 16,
                                                                 },
                                                                 new LayerParameters
                                                                 {
                                                                     VisibleLayerType = LayerType.Gaussian,
                                                                     HiddenLayerType = LayerType.Gaussian,
                                                                     Neurons = 16,
                                                                 },
                                                             };

        [TunableParameter]
        public int BatchSize { get; set; } = 150;

        [TunableParameter]
        public double LearningRate { get; set; } = 0.15;

        [TunableParameter(Min = 0d, Max = 1d)]
        public double Momentum { get; set; } = 0.2;

        [TunableParameter]
        public int Epochs { get; set; } = 150;

        [TunableParameter]
        public WeightInitializationType InitializationType { get; set; } = WeightInitializationType.NguyenWidrow;

        public DeepBeliefNetworkSettings() { }

        public DeepBeliefNetworkSettings(int numHiddenLayers, int numIterations)
        {
            NumIterations = numIterations;
            var hiddenLayers = new List<LayerParameters>(numHiddenLayers);
            for (int i = 0; i < numHiddenLayers + 1; i++)
                hiddenLayers.Add(new LayerParameters());
            Layers = hiddenLayers;
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Number of Iterations: {NumIterations}");
            sb.AppendLine($"Batch Size: {BatchSize}");
            sb.AppendLine($"Network Learning Rate: {LearningRate}");
            sb.AppendLine($"Network Momentum: {Momentum}");
            sb.AppendLine($"Network Epochs: {Epochs}");
            sb.AppendLine($"Network Initialization: {InitializationType}");
            sb.AppendLine();

            for (int i = 0; i < Layers.Count; i++)
            {
                sb.AppendLine($"Layer {i}:");
                sb.AppendLine(Layers[i].ToString());
            }

            return sb.ToString();
        }

        #endregion
    }
}

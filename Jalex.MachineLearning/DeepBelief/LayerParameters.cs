using System;
using System.Text;
using Accord.Neuro.ActivationFunctions;
using Jalex.MachineLearning.Tuning;

namespace Jalex.MachineLearning.DeepBelief
{
    public class LayerParameters
    {
        [TunableParameter]
        public int Neurons { get; set; } = 64;

        [TunableParameter]
        public double LearningRate { get; set; } = 0.1;

        [TunableParameter(Min = 0d, Max = 1d)]
        public double Momentum { get; set; } = 0.5;

        [TunableParameter]
        public double Decay { get; set; } = 0.001;

        [TunableParameter]
        public int Epochs { get; set; } = 100;

        [TunableParameter]
        public double Alpha { get; set; } = 1;

        [TunableParameter]
        public LayerType HiddenLayerType { get; set; } = LayerType.Bernoulli;

        [TunableParameter]
        public LayerType VisibleLayerType { get; set; } = LayerType.Bernoulli;

        public IStochasticFunction GetVisibleActivationFunction()
        {
            return getActivationFunctionForLayerType(VisibleLayerType);
        }

        public IStochasticFunction GetHiddenActivationFunction()
        {
            return getActivationFunctionForLayerType(HiddenLayerType);
        }

        private IStochasticFunction getActivationFunctionForLayerType(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.Bernoulli:
                    return new BernoulliFunction(Alpha);
                case LayerType.Gaussian:
                    return new GaussianFunction(Alpha);
                default:
                    throw new IndexOutOfRangeException("Unknown layer type " + layerType);
            }
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Neurons: {Neurons}");
            sb.AppendLine($"Learning Rate: {LearningRate}");
            sb.AppendLine($"Momentum: {Momentum}");
            sb.AppendLine($"Decay: {Decay}");
            sb.AppendLine($"Alpha: {Alpha}");
            sb.AppendLine($"Visible Layer Type: {VisibleLayerType}");
            sb.AppendLine($"Hidden Layer Type: {HiddenLayerType}");
            sb.AppendLine($"Epochs: {Epochs}");

            return sb.ToString();
        }

        #endregion
    }
}

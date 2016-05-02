using System;

namespace Jalex.MachineLearning.Extractors
{
    public class LambdaInputExtractor<TInput> : IInputExtractor<TInput>
    {
        private readonly Func<TInput, double[]> _inputExtractionFunc;

        public LambdaInputExtractor(Func<TInput, double[]> inputExtractionFunc)
        {
            if (inputExtractionFunc == null) throw new ArgumentNullException(nameof(inputExtractionFunc));
            _inputExtractionFunc = inputExtractionFunc;
        }

        #region Implementation of IInputExtractor

        public double[] ExtractInputs(TInput input) => _inputExtractionFunc(input);

        #endregion
    }
}

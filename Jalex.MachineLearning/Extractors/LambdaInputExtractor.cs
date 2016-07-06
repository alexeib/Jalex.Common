using System;

namespace Jalex.MachineLearning.Extractors
{
    public class LambdaInputExtractor<TInstance, TInput> : IInputExtractor<TInstance, TInput>
    {
        private readonly Func<TInstance, TInput[]> _inputExtractionFunc;

        public LambdaInputExtractor(Func<TInstance, TInput[]> inputExtractionFunc)
        {
            if (inputExtractionFunc == null) throw new ArgumentNullException(nameof(inputExtractionFunc));
            _inputExtractionFunc = inputExtractionFunc;
        }

        #region Implementation of IInputExtractor

        public TInput[] ExtractInputs(TInstance input) => _inputExtractionFunc(input);

        #endregion
    }
}

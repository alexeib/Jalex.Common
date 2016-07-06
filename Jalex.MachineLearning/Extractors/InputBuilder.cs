using System.Collections.Generic;
using System.Linq;

namespace Jalex.MachineLearning.Extractors
{
    internal class InputBuilder<TInstance, TInput>
    {
        private readonly IInputExtractor<TInstance, TInput> _inputExtractor;

        public InputBuilder(IInputExtractor<TInstance, TInput> inputExtractor)
        {
            _inputExtractor = inputExtractor;
        }

        public TInput[][] BuildInputs(IEnumerable<TInstance> instances)
        {
            var inputs = toInputs(instances);
            return inputs.ToArray();
        }

        private IEnumerable<TInput[]> toInputs(IEnumerable<TInstance> instances)
        {
            return from instance in instances
                   let numericalInputs = _inputExtractor.ExtractInputs(instance)
                   where numericalInputs != null
                   select numericalInputs;
        }
    }
}

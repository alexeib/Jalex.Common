using System;
using System.Collections.Generic;
using Jalex.MachineLearning.Tuning.Tuners;
using NLog;

namespace Jalex.MachineLearning.Tuning
{
    internal class PropertyMover
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IValueMover _valueMover;
        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;
        private readonly Func<object, object> _clone;

        public string Name { get; }

        public PropertyMover(string name, IValueMover valueMover, Func<object, object> getValue, Action<object, object> setValue, Func<object, object> clone)
        {
            Name = name;
            _valueMover = valueMover;
            _getValue = getValue;
            _setValue = setValue;
            _clone = clone;
        }

        public IEnumerable<TParameters> GetMovedParameters<TParameters>(TParameters parameters)
        {
            var initialValue = _getValue(parameters);
            var movedValues = _valueMover.Values(initialValue);
            foreach (var movedValue in movedValues)
            {
                _logger.Info($"Moved value {initialValue} to {movedValue}");

                var cloned = (TParameters)_clone(parameters);
                _setValue(cloned, movedValue);
                yield return cloned;
            }
        }

        public object GetValue(object parameters) => _getValue(parameters);
    }
}

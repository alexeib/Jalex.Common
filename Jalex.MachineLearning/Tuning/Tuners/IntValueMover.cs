using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class IntValueMover : IValueMover
    {
        const int _zeroStep = 1;
        const double _percentMove = 0.5;

        private readonly Range<int> _range;

        public IntValueMover(Range<int> range)
        {
            _range = range;
        }

        #region Implementation of IValueMover

        public IEnumerable<object> Values(object initialValue)
        {
            if (!(initialValue is int))
                throw new InvalidOperationException("initial value must be of type int");

            var value = (int)initialValue;
            if (value == 0)
            {
                if (_zeroStep >= _range.Minimum && _zeroStep <= _range.Maximum)
                {
                    yield return _zeroStep;
                }
                if (-_zeroStep >= _range.Minimum && -_zeroStep <= _range.Maximum)
                {
                    yield return -_zeroStep;
                }
            }
            else
            {
                var move = (int)Math.Ceiling(value * _percentMove);
                if (value + move >= _range.Minimum && value + move <= _range.Maximum)
                {
                    yield return value + move;
                }
                if (value - move >= _range.Minimum && value - move <= _range.Maximum)
                {
                    yield return value - move;
                }
            }
        }

        #endregion
    }
}

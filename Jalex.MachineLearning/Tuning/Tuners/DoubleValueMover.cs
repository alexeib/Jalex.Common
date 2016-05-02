using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class DoubleValueMover : IValueMover
    {
        private readonly Range<double> _range;

        const double _zeroStep = 1d;
        const double _percentMove = 0.5;

        public DoubleValueMover(Range<double> range)
        {
            _range = range;
        }

        #region Implementation of IValueMover

        public IEnumerable<object> Values(object initialValue)
        {
            if (!(initialValue is double))
                throw new InvalidOperationException("initial value must be of type double");

            var value = (double)initialValue;

            if (value == 0)
            {
                if (_zeroStep >= _range.Minimum && _zeroStep <= _range.Maximum)
                {
                    yield return _zeroStep;
                }
                if(-_zeroStep >= _range.Minimum && -_zeroStep <= _range.Maximum)
                {
                    yield return -_zeroStep;
                }
            }
            else
            {
                var move = value * _percentMove;

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

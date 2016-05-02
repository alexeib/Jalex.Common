using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class TimeSpanValueMover : IValueMover
    {
        private static readonly TimeSpan _zeroStep = TimeSpan.FromDays(1);
        const double _percentDayMove = 0.5;

        #region Implementation of IValueMover

        public IEnumerable<object> Values(object initialValue)
        {
            if (!(initialValue is TimeSpan))
                throw new InvalidOperationException("initial value must be of type int");

            var value = (TimeSpan)initialValue;
            if (value == TimeSpan.Zero)
            {
                yield return _zeroStep;
            }
            else
            {
                var move = Math.Ceiling(value.TotalDays * _percentDayMove);
                yield return TimeSpan.FromDays(value.TotalDays + move);

                if (move < value.TotalDays)
                {
                    yield return TimeSpan.FromDays(value.TotalDays - move);
                }
            }
        }

        #endregion
    }
}

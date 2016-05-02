using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class BooleanValueMover : IValueMover
    {
        #region Implementation of IValueMover

        public IEnumerable<object> Values(object initialValue)
        {
            if (!(initialValue is bool))
                throw new InvalidOperationException("initial value must be of type double");

            var value = (bool) initialValue;
            yield return !value;
        }

        #endregion
    }
}

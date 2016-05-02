using System;
using System.Collections.Generic;
using System.Linq;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class EnumValueMover : IValueMover
    {
        #region Implementation of IValueMover

        public IEnumerable<object> Values(object initialValue)
        {
            if (!(initialValue is Enum))
                throw new InvalidOperationException("initial value must be of type double");

            return initialValue.GetType()
                               .GetEnumValues()
                               .Cast<object>()
                               .Where(enumVal => !enumVal.Equals(initialValue));
        }

        #endregion
    }
}

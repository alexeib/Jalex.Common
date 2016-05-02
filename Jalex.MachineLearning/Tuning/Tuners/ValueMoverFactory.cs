using System;
using System.Reflection;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public class ValueMoverFactory : IValueMoverFactory
    {
        public IValueMover CreateValueMover(PropertyInfo property)
        {
            if(property.PropertyType == typeof(bool))
                return new BooleanValueMover();
            if(property.PropertyType.IsEnum)
                return new EnumValueMover();
            if (property.PropertyType == typeof(TimeSpan))
                return new TimeSpanValueMover();
            if (property.PropertyType == typeof (double))
                return new DoubleValueMover(getDoubleRange(property));
            if (property.PropertyType == typeof(int))
                return new IntValueMover(getIntRange(property));

            return null;
        }

        private Range<double> getDoubleRange(PropertyInfo property)
        {
            double min = 0;
            double max = double.MaxValue;
            var tunableAttribute = property.GetCustomAttribute<TunableParameter>();
            if (tunableAttribute != null)
            {
                min = (double?) tunableAttribute.Min ?? 0;
                max = (double?)tunableAttribute.Max ?? double.MaxValue;
            }

            return new Range<double>(min, max);
        }

        private Range<int> getIntRange(PropertyInfo property)
        {
            int min = 0;
            int max = int.MaxValue;
            var tunableAttribute = property.GetCustomAttribute<TunableParameter>();
            if (tunableAttribute != null)
            {
                min = (int?)tunableAttribute.Min ?? 0;
                max = (int?)tunableAttribute.Max ?? int.MaxValue;
            }

            return new Range<int>(min, max);
        }
    }
}

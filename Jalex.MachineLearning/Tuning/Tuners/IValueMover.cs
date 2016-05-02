using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public interface IValueMover
    {
        IEnumerable<object> Values(object initialValue);
    }
}
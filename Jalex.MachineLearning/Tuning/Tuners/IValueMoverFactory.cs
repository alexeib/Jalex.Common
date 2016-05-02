using System.Reflection;

namespace Jalex.MachineLearning.Tuning.Tuners
{
    public interface IValueMoverFactory
    {
        IValueMover CreateValueMover(PropertyInfo property);
    }
}
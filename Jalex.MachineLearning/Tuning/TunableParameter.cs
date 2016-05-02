using System;

namespace Jalex.MachineLearning.Tuning
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TunableParameter: Attribute
    {
        public object Min { get; set; }
        public object Max { get; set; }
    }
}

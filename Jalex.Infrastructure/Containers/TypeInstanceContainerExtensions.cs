using System.Collections.Generic;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Containers
{
    public static class TypeInstanceContainerExtensions
    {
        public static void SetMany<T>(this TypeInstanceContainer<T> metricContainer, IEnumerable<T> metrics) where T : class
        {
            ParameterChecker.CheckForNull(metricContainer, "metricContainer");
            ParameterChecker.CheckForNull(metrics, "metrics");

            foreach (var metric in metrics)
            {
                metricContainer.Set(metric);
                ;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Itenso.TimePeriod;

namespace Jalex.Infrastructure.Extensions
{
    public static class TimePeriodExtensions
    {
        public static void ForEachDay(
            this IEnumerable<ITimePeriod> timePeriods,
            Action<DateTime> action)
        {
            foreach (var timePeriod in timePeriods)
            {
                if (!timePeriod.HasStart)
                {
                    throw new InvalidOperationException("time period must have a start");
                }

                if (!timePeriod.HasEnd)
                {
                    throw new InvalidOperationException("time period must have an end");
                }

                DateTime currDate = timePeriod.Start;
                while (currDate <= timePeriod.End)
                {
                    action(currDate);
                    currDate = currDate.AddDays(1);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Itenso.TimePeriod;

namespace Jalex.Infrastructure.Extensions
{
    public static class TimePeriodExtensions
    {
        public static void ForEachDay(this IEnumerable<ITimePeriod> timePeriods, Action<DateTime> action)
        {
            foreach (var date in timePeriods.Dates())
            {
                action(date);
            }
        }

        public static IEnumerable<DateTime> Dates(this IEnumerable<ITimePeriod> timePeriods)
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
                    yield return currDate;
                    currDate = currDate.AddDays(1);
                }
            }
        }
    }
}

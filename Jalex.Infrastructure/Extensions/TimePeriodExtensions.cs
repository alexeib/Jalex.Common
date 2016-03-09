using System;
using System.Collections.Generic;
using System.Linq;
using Itenso.TimePeriod;

namespace Jalex.Infrastructure.Extensions
{
    public static class TimePeriodExtensions
    {
        public static void ForEachDay(this IEnumerable<ITimePeriod> timePeriods, bool distinct, Action<DateTime> action)
        {
            foreach (var date in timePeriods.Dates(distinct))
            {
                action(date);
            }
        }

        public static ITimePeriodCollection Merge(this IEnumerable<ITimePeriod> timePeriods)
        {            
            var merged = new TimePeriodCollection();

            var sorted = timePeriods.OrderBy(p => p.Start)
                                    .ToList();

            if (sorted.Count == 0)
                return merged;

            DateTime currStart = sorted[0].Start;
            DateTime currEnd = sorted[0].End;
            

            foreach (var next in sorted.Skip(1))
            {
                if (next.Start > currEnd)
                {
                    merged.Add(new TimeBlock(currStart, currEnd));
                    currStart = next.Start;
                    currEnd = next.End;
                }
                else if (next.End > currEnd)
                {
                    currEnd = next.End;
                }
            }

            merged.Add(new TimeBlock(currStart, currEnd));

            return merged;
        }

        public static IEnumerable<DateTime> Dates(this IEnumerable<ITimePeriod> timePeriods, bool distinct)
        {
            if (distinct)
            {
                timePeriods = timePeriods.Merge();
            }

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

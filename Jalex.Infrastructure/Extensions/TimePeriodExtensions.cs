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

            return timePeriods.SelectMany(timePeriod => timePeriod.Dates());
        }

        public static IEnumerable<DateTime> Dates(this ITimePeriod timePeriod)
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

        public static ITimePeriodCollection Without(this IEnumerable<ITimePeriod> timePeriods, IEnumerable<ITimePeriod> toRemove)
        {
            TimePeriodCollection resultingCollection = new TimePeriodCollection();

            var removePeriods = toRemove.Merge()
                                        .OrderBy(r => r.Start)
                                        .ToArray();
            var periods = timePeriods.Merge()
                                     .OrderBy(r => r.Start);

            int removePeriodsIdx = 0;
            foreach (var period in periods)
            {
                while (removePeriodsIdx < removePeriods.Length && removePeriods[removePeriodsIdx].End < period.Start)
                {
                    removePeriodsIdx++;
                }

                var currPeriod = period;

                while (currPeriod != null)
                {
                    if (removePeriodsIdx >= removePeriods.Length || removePeriods[removePeriodsIdx].Start > currPeriod.End)
                    {
                        resultingCollection.Add(currPeriod);
                        currPeriod = null;
                    }
                    else
                    {
                        var rem = removePeriods[removePeriodsIdx];
                        if (rem.Start > currPeriod.Start)
                        {
                            resultingCollection.Add(new TimeBlock(currPeriod.Start, rem.Start.AddDays(-1)));
                        }
                        if (rem.End < period.End)
                        {
                            currPeriod = new TimeBlock(rem.End.AddDays(1), period.End);
                            removePeriodsIdx++;
                        }
                        else
                        {
                            currPeriod = null;
                        }
                    }
                }
            }

            return resultingCollection;
        }
    }
}

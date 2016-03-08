using System;
using Itenso.TimePeriod;
using Jalex.Infrastructure.Extensions;

namespace Jalex.Infrastructure.Utils
{
    public static class TimePeriodFactory
    {
        public static ITimePeriodCollection FromStartEnd(DateTime startDate, DateTime endDate)
        {
            return new TimePeriodCollection(new TimeBlock(startDate, endDate).ToEnumerable());
        }

        public static ITimePeriodCollection FromStartEnd(DateTime startDate, DateTime endDate, DateTimeKind kind)
        {
            return FromStartEnd(DateTime.SpecifyKind(startDate, kind), DateTime.SpecifyKind(endDate, kind));
        }
    }
}

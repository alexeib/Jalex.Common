using System;
using System.Collections.Generic;
using FluentAssertions;
using Itenso.TimePeriod;

namespace Jalex.TestUtils
{
    public static class DateTestUtils
    {
        public static void TestForEachDate(
            IEnumerable<ITimePeriod> timePeriods,
            Action<DateTime> test)
        {
            foreach (var timePeriod in timePeriods)
            {
                timePeriod.HasStart.Should().BeTrue();
                timePeriod.HasEnd.Should().BeTrue();

                DateTime currDate = timePeriod.Start;
                while (currDate <= timePeriod.End)
                {
                    test(currDate);
                    currDate = currDate.AddDays(1);
                }
            }
        }
    }
}

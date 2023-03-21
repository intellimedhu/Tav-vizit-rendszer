using IntelliMed.Core.Extensions;
using System;
using Xunit;

namespace IntelliMedCore.Tests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ToShortTime()
        {
            var t = TimeSpan.FromMinutes(60 * 7.5);
            var result = t.ToShortTime();
            Assert.Equal("07:30", result);
        }

        [Theory]
        [InlineData("2019-08-04", "2019-07-29")]
        [InlineData("2019-08-05", "2019-08-05")]
        [InlineData("2019-08-06", "2019-08-05")]
        [InlineData("2019-08-07", "2019-08-05")]
        [InlineData("2019-08-08", "2019-08-05")]
        [InlineData("2019-08-09", "2019-08-05")]
        [InlineData("2019-08-10", "2019-08-05")]
        [InlineData("2019-08-11", "2019-08-05")]
        [InlineData("2019-08-12", "2019-08-12")]
        public void GetWeekStart(string inputDate, string expected)
        {
            var date = DateTime.Parse(inputDate);
            var result = date.GetWeekStart();
            Assert.Equal(DateTime.Parse(expected), result);
        }

        [Theory]
        [InlineData("2000-05-05", "2000-05-01", "2000-05-10", false, false, true)]
        [InlineData("2000-05-05 10:00", "2000-05-05 10:00", "2000-05-10", true, true, true)]
        [InlineData("2000-05-05 10:00", "2000-05-05 10:00", "2000-05-10", false, true, false)]
        [InlineData("2000-05-05 12:13", "2000-05-01", "2000-05-05 12:13", false, true, true)]
        [InlineData("2000-05-05 12:13", "2000-05-01", "2000-05-05 12:13", false, false, false)]
        public void IsBetweenDateRange_ShouldReturn_AsExpected(
            string inputStr,
            string startDateStr,
            string endDateStr,
            bool includeStart,
            bool includeEnd,
            bool expectedResult)
        {
            var result = DateTime.Parse(inputStr)
                .IsBetweenDateRange(
                    DateTime.Parse(startDateStr),
                    DateTime.Parse(endDateStr), includeStart, includeEnd);

            Assert.Equal(expectedResult, result);
        }
    }
}

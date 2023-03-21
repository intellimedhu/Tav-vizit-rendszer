using System;

namespace IntelliMed.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToShortTime(this TimeSpan timeSpan)
            => timeSpan.ToString(@"hh\:mm");

        public static DateTime GetWeekStart(this DateTime dateTime)
        {
            dateTime.ThrowIfNull();

            return dateTime.Date.AddDays(-(dateTime.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)dateTime.DayOfWeek - 1));
        }

        public static bool IsBetweenDateRange(
            this DateTime subjectDate,
            DateTime startDate,
            DateTime endDate,
            bool inclusiveStartDate = true,
            bool inclusiveEndDate = false)
        {
            var overStartDate = inclusiveStartDate
                ? startDate <= subjectDate
                : startDate < subjectDate;

            if (!overStartDate)
            {
                return false;
            }

            return inclusiveEndDate
                ? subjectDate <= endDate
                : subjectDate < endDate;
        }
    }
}

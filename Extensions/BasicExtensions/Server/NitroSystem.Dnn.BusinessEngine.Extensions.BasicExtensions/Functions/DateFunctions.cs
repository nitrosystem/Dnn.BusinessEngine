using System;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Functions
{
    public static class ParticipantsHtml
    {
        [ExpressionFunction("DateTimeToRelative")]
        public static string ParseRelativeDate(DateTime date)
        {
            var now = DateTime.Now;
            var ts = now - date;

            // Past
            if (ts.TotalSeconds < 60)
                return "a few seconds ago";
            if (ts.TotalMinutes < 2)
                return "one minute ago";
            if (ts.TotalMinutes < 60)
                return $"{ts.Minutes} minutes ago";
            if (ts.TotalHours < 2)
                return "an hour ago";
            if (ts.TotalHours < 24)
                return $"{ts.Hours} hours ago";
            if (ts.TotalDays < 2)
                return "yesterday";
            if (ts.TotalDays < 30)
                return $"{ts.Days} days ago";
            if (ts.TotalDays < 365)
                return $"{ts.Days / 30} months ago";

            return $"{ts.Days / 365} years ago";
        }

        [ExpressionFunction("DateTimeFormat")]
        public static string ParseDateTimeFormat(DateTime date, string format)
        {
            return date.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}

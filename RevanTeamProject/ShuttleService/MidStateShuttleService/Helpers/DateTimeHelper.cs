using System;

namespace MidStateShuttleService.Helpers
{
    // Handles converting UTC timestamps to Wisconsin (Central) time for display
    public static class DateTimeHelper
    {
        // Central Time zone (auto handles daylight savings)
        private static readonly TimeZoneInfo CentralTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        // Convert UTC DateTime → Central Time string
        public static string ToCentralTimeString(DateTime utcDateTime, string format = "MM/dd/yyyy h:mm tt")
        {
            // Ensure value is treated as UTC
            var utc = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            // Convert to Central (WI time)
            var centralTime = TimeZoneInfo.ConvertTimeFromUtc(utc, CentralTimeZone);

            return centralTime.ToString(format);
        }

        // Overload for nullable DateTime (prevents null crashes)
        public static string ToCentralTimeString(DateTime? utcDateTime, string format = "MM/dd/yyyy h:mm tt")
        {
            if (!utcDateTime.HasValue)
                return "";

            return ToCentralTimeString(utcDateTime.Value, format);
        }
    }
}
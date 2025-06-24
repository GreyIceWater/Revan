namespace MidStateShuttleService.Services
{
    public static class TimeService
    {
        private static readonly TimeZoneInfo CentralZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        public static DateTime ConvertUtcToCentral(DateTime utcDate)
        {
            var utc = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, CentralZone);
        }

        public static DateTime ConvertCentralToUtc(DateTime centralDate)
        {
            var central = DateTime.SpecifyKind(centralDate, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(central, CentralZone);
        }
    }
}
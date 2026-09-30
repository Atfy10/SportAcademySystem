namespace SportAcademy.Domain.Services
{
    // "Today" as the academy sees it - its own time zone, not UTC. A subscription that starts
    // today must show as Active from the academy's midnight; with UTC dates a Kuwait (UTC+3)
    // academy still saw it as Upcoming until 03:00. Set once per request (after the tenant is
    // resolved) and per tenant inside background sweeps - the same ambient AsyncLocal pattern
    // as ITenantIdProvider. Falls back to the UTC date when nothing has set it.
    public static class TenantCalendar
    {
        private static readonly AsyncLocal<DateOnly?> Current = new();

        public static DateOnly Today => Current.Value ?? DateOnly.FromDateTime(DateTime.UtcNow);

        public static void Set(DateOnly? today) => Current.Value = today;

        public static DateOnly TodayIn(TimeZoneInfo? timeZone)
            => DateOnly.FromDateTime(timeZone is null ? DateTime.UtcNow : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        // An unknown / invalid id is treated as UTC rather than failing whatever asked.
        public static TimeZoneInfo? FindTimeZone(string? timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) return null;
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                return null;
            }
        }
    }
}

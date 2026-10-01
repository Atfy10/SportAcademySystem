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
        private static readonly AsyncLocal<TimeZoneInfo?> Zone = new();

        public static DateOnly Today => Current.Value ?? DateOnly.FromDateTime(DateTime.UtcNow);

        public static void Set(DateOnly? today) => Current.Value = today;

        // The academy's configured time zone (TenantSettings.TimeZone), set per request next to
        // Today. Null (no tenant, unknown id, or a test that never set it) behaves as UTC.
        public static TimeZoneInfo? TimeZone => Zone.Value;

        public static void SetTimeZone(TimeZoneInfo? timeZone) => Zone.Value = timeZone;

        /// <summary>A stored UTC instant as the academy's wall-clock time. The result is always
        /// Kind.Unspecified, so it serializes with no offset ("2026-10-05T18:00:00") and is read
        /// back by the browser exactly as shown.</summary>
        public static DateTime ToLocal(DateTime utc)
        {
            var asUtc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            var local = Zone.Value is { } zone ? TimeZoneInfo.ConvertTimeFromUtc(asUtc, zone) : asUtc;
            return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        }

        /// <summary>A time the academy's staff entered (their wall clock) as a UTC instant. A value
        /// that already says it's UTC ("...Z") is taken as-is. A wall-clock time that doesn't exist
        /// (skipped by a daylight-saving jump) is moved forward past the gap.</summary>
        public static DateTime ToUtc(DateTime local)
        {
            if (local.Kind == DateTimeKind.Utc) return local;
            var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
            if (Zone.Value is not { } zone) return DateTime.SpecifyKind(unspecified, DateTimeKind.Utc);
            if (zone.IsInvalidTime(unspecified)) unspecified = unspecified.AddHours(1);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, zone);
        }

        /// <summary>The UTC instant the academy's calendar day begins.</summary>
        public static DateTime DayStartUtc(DateOnly day) => ToUtc(day.ToDateTime(TimeOnly.MinValue));

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

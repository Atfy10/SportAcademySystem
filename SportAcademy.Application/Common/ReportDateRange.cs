namespace SportAcademy.Application.Common
{
    public static class ReportDateRange
    {
        // A "to" filter arrives from the UI as a bare date (midnight). Compared with <= against
        // timestamps it silently drops everything that happened later that same day, so a
        // date-only bound is widened to the start of the next day and compared with <.
        // A bound that carries a time of day is honoured as given.
        public static DateTime EndExclusive(DateTime to)
            => to.TimeOfDay == TimeSpan.Zero ? to.Date.AddDays(1) : to.AddTicks(1);
    }
}

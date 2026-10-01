using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Services
{
    // An event's lifecycle status is derived from its dates, never stored - same approach as a
    // subscription's effective status. Day-granular against the academy's own "today"
    // (TenantCalendar.Today): an event running on any part of today is Ongoing. startsAt/endsAt
    // are the academy's local wall-clock times (convert stored UTC with TenantCalendar.ToLocal).
    public static class EventStatusRules
    {
        public static EventStatus Resolve(bool isCancelled, DateTime startsAtLocal, DateTime endsAtLocal, DateOnly today)
        {
            if (isCancelled) return EventStatus.Cancelled;
            if (DateOnly.FromDateTime(startsAtLocal) > today) return EventStatus.Upcoming;
            if (DateOnly.FromDateTime(endsAtLocal) < today) return EventStatus.Completed;
            return EventStatus.Ongoing;
        }

        // The same boundaries as UTC instants, so list/report filters (which compare the stored
        // UTC columns) and Resolve always agree: Upcoming = StartsAt >= TomorrowStart; Completed
        // = EndsAt < TodayStart; Ongoing = neither.
        public static (DateTime TodayStart, DateTime TomorrowStart) DayBounds(DateOnly today)
            => (TenantCalendar.DayStartUtc(today), TenantCalendar.DayStartUtc(today.AddDays(1)));
    }
}

using System.Security.Cryptography;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Services
{
    // The rules for the event entry QR code: when it lets people in, and how its code is made.
    public static class EventEntryRules
    {
        // Guests can be let in from this long before the event starts until it ends.
        public static readonly TimeSpan OpensBefore = TimeSpan.FromMinutes(15);

        // 128 random bits as 32 lowercase hex characters - unguessable, and safe in a URL path.
        public static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

        public static DateTime OpensAt(DateTime startsAtUtc) => startsAtUtc - OpensBefore;

        // The reason a scan can't let anyone in at this moment, or null when entry is open.
        // Everything is compared as UTC instants.
        public static EventEntryResult? Closed(bool isCancelled, DateTime startsAtUtc, DateTime endsAtUtc, DateTime nowUtc)
        {
            if (isCancelled) return EventEntryResult.Cancelled;
            if (nowUtc < OpensAt(startsAtUtc)) return EventEntryResult.NotYetOpen;
            if (nowUtc >= endsAtUtc) return EventEntryResult.Ended;
            return null;
        }
    }
}

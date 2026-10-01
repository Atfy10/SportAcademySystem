using System.Security.Cryptography;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Services
{
    // The rules for event entry tickets: when they let people in, when they die, and how their
    // codes are made.
    public static class EventEntryRules
    {
        // Guests can be let in from this long before the event starts until it ends.
        public static readonly TimeSpan OpensBefore = TimeSpan.FromMinutes(15);

        // One "issue tickets" request can't make more than this many at once.
        public const int MaxTicketsPerIssue = 1000;

        // Ticket numbers are random 6-digit numbers, unique within their event - short enough
        // for door staff to type when a code won't scan, and not sequential, so nobody can guess
        // another guest's ticket by counting up from their own. (The capacity limit of 100,000
        // keeps an event well inside the 900,000 possible numbers.)
        public const int TicketNumberMin = 100_000;
        public const int TicketNumberMax = 999_999;

        // 128 random bits as 32 lowercase hex characters - unguessable, and safe in a URL path.
        public static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

        public static DateTime OpensAt(DateTime startsAtUtc) => startsAtUtc - OpensBefore;

        // A random ticket number not in `taken` (which it then adds to, so a batch never repeats one).
        public static int NewTicketNumber(ISet<int> taken)
        {
            while (true)
            {
                var number = RandomNumberGenerator.GetInt32(TicketNumberMin, TicketNumberMax + 1);
                if (taken.Add(number)) return number;
            }
        }

        // True once the event has ended or been cancelled. From then on every one of its tickets
        // is dead for good: it can't be admitted, issued, re-issued, renamed or revoked, and the
        // guest's ticket page stops showing the code. (UpdateEvent refuses to move an ended
        // event's times, so this can't be undone by editing the event.)
        public static bool TicketsTerminated(bool isCancelled, DateTime endsAtUtc, DateTime nowUtc)
            => isCancelled || nowUtc >= endsAtUtc;

        // The reason a ticket can't be let in at this moment, or null when entry is open.
        // Everything is compared as UTC instants.
        public static EventTicketCheckResult? Closed(bool isCancelled, DateTime startsAtUtc, DateTime endsAtUtc, DateTime nowUtc)
        {
            if (isCancelled) return EventTicketCheckResult.Cancelled;
            if (nowUtc >= endsAtUtc) return EventTicketCheckResult.Ended;
            if (nowUtc < OpensAt(startsAtUtc)) return EventTicketCheckResult.NotYetOpen;
            return null;
        }
    }
}

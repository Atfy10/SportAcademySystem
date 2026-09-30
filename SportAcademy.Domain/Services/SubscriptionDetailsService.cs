using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public class SubscriptionDetailsService
    {
        // Sessions for the WHOLE subscription, not one month of it. DaysPerMonth is a monthly
        // rate ("Quarterly" is seeded as DaysPerMonth 10 x NumberOfMonths 3), so returning it
        // alone granted a three-month subscriber a single month's sessions - while the price
        // they paid, the end date counted across their training days, and the total shown on the
        // subscription form were all for the full term.
        public static int CalculateAllowedSessions(SubscriptionDetails subscriptionDetails)
        {
            var subscriptionType = subscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType;
            return TrainingScheduleService.CalculateTotalSessions(
                subscriptionType.DaysPerMonth, subscriptionType.NumberOfMonths);
        }

        public static bool HasActiveSubscriptionConflict(
            SubscriptionDetails subDetails,
            List<SubscriptionDetails>? activeSubscriptions
            )
        {
            if (!(activeSubscriptions == null || activeSubscriptions.Count == 0))
            {
                foreach (var activeSub in activeSubscriptions)
                {
                    if ((subDetails.StartDate <= activeSub.EndDate
                        && subDetails.EndDate >= activeSub.StartDate)
                        && subDetails.SportId == activeSub.SportId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // The trainee's same-sport subscription a new (or re-dated) one would clash with: any other
        // subscription in that sport that ends on or after the new start date - i.e. one that
        // overlaps it, or one that starts later (a new subscription can't be slotted in before an
        // existing/upcoming one). Returns the one ending last, so the message can say from when a
        // new subscription would be accepted. Any status counts: a suspended subscription still
        // owns its period.
        public static SubscriptionDetails? FindSameSportConflict(
            SubscriptionDetails candidate, IEnumerable<SubscriptionDetails>? existing)
            => existing?
                .Where(e => e.Id != candidate.Id
                    && e.SportId == candidate.SportId
                    && !e.IsDeleted
                    && e.EndDate >= candidate.StartDate)
                .OrderByDescending(e => e.EndDate)
                .FirstOrDefault();

        public static bool IsSubscriptionActive(SubscriptionDetails subscriptionDetails)
            => subscriptionDetails.EndDate >= TenantCalendar.Today
            && subscriptionDetails.StartDate <= TenantCalendar.Today;

        // Distinct from "not currently active": a subscription that hasn't started yet
        // (StartDate in the future) isn't active *today* either, but it hasn't expired - it's
        // upcoming. Only a subscription whose end date has actually passed has expired. Callers
        // deciding whether to stamp Status = Expired at creation/update time must use this, not
        // !IsSubscriptionActive, or a future-dated (e.g. a renewal starting tomorrow) subscription
        // gets mislabeled Expired the moment it's created.
        public static bool HasExpired(SubscriptionDetails subscriptionDetails)
            => subscriptionDetails.EndDate < TenantCalendar.Today;
    }
}

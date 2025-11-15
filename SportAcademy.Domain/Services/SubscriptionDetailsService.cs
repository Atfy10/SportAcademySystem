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
        public static int CalculateAllowedSessions(SubscriptionDetails subscriptionDetails)
            => subscriptionDetails.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth;

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
                        || subDetails.EndDate >= activeSub.StartDate)
                        && subDetails.SportId == activeSub.SportId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsSubscriptionActive(SubscriptionDetails subscriptionDetails)
            => subscriptionDetails.EndDate >= DateOnly.FromDateTime(DateTime.Now)
            && subscriptionDetails.StartDate <= DateOnly.FromDateTime(DateTime.Now);
    }
}

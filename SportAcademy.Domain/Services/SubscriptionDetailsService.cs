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
            => subscriptionDetails.SportSubscriptionType.SubscriptionType.DaysPerMonth;
    }
}

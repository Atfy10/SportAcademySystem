using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class SubscriptionType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int DaysPerMonth { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsOffer { get; set; } = false;
    }
}

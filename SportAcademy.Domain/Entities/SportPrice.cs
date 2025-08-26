using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class SportPrice
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public int SubsTypeId { get; set; }
        public decimal Price { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual SubscriptionType SubscriptionType { get; set; }
    }
}

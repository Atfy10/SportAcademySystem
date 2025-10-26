using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class Payment : IAuditableEntity
    {
        public required string PaymentNumber { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaidDate { get; set; } = DateTime.Now;
        public int BranchId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual SubscriptionDetails SubscriptionDetails { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Payment
    {
        public required string PaymentNumber { get; set; }
        public required string Method { get; set; }
        public DateTime PaidDate { get; set; } = DateTime.Now;
        // nav For Branch 
        public  int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }


    }
}

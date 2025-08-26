using System;
using System.Collections.Generic;
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

    }
}

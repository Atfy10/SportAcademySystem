using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.PaymentDtos
{
    public record PaymentSubDetailsDto
    {
        public string PaymentNumber { get; init; } = null!;
        public DateTime PaidDate { get; init; }
        public string BranchName { get; init; } = null!;
        public PaymentMethod PaymentMethod { get; init; }
    }
}

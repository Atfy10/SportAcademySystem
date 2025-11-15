using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.PaymentDtos
{
    public record PaymentSubDetailsDto(
        string PaymentNumber,
        DateTime PaidDate,
        string BranchName,
        PaymentMethod PaymentMethod
    );
}

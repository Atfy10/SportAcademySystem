using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public class SubscriptionDetailsDto
    {
        public TraineeSubDetailsDto Trainee { get; set; } = null!;
        public string SportName { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public string SubscriptionTypeName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EmployeeName { get; set; } = null!;
        public PaymentSubDetailsDto Payment { get; set; } = null!;
    }
}

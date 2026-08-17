using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for the AutoMapper SubscriptionDetails -> SubscriptionDetailsDto
    // mapping (SubscriptionDetailsProfile.cs), used only by the new paginated/search query -
    // the existing unpaginated GetAllSubDetailsQuery keeps using AutoMapper untouched.
    public static class SubscriptionDetailsMapper
    {
        public static SubscriptionDetailsDto ToDto(SubscriptionDetails sd) => new()
        {
            Id = sd.Id,
            Trainee = new TraineeSubDetailsDto
            {
                Id = sd.Trainee.Id,
                FullName = $"{sd.Trainee.FirstName} {sd.Trainee.LastName}",
                PhoneNumber = sd.Trainee.PhoneNumber,
            },
            SportName = sd.SportPrice.SportSubscriptionType.Sport.Name,
            BranchName = sd.SportPrice.Branch.Name,
            SubscriptionTypeName = sd.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString(),
            Price = sd.SportPrice.Price,
            StartDate = sd.StartDate,
            EndDate = sd.EndDate,
            Payment = new PaymentSubDetailsDto
            {
                PaymentNumber = sd.Payment.PaymentNumber,
                PaidDate = sd.Payment.PaidDate,
                BranchName = sd.Payment.Branch.Name,
                PaymentMethod = sd.Payment.Method,
            },
            Status = sd.Status,
        };
    }
}

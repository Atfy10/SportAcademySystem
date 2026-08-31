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
        public static SubscriptionDetailsDto ToDto(SubscriptionDetails sd, string lang)
        {
            var latestPayment = sd.InvoiceLines
                .SelectMany(l => l.Invoice.Allocations)
                .Select(a => a.Payment)
                .OrderByDescending(p => p.PaidDate)
                .FirstOrDefault();

            return new SubscriptionDetailsDto
            {
                Id = sd.Id,
                Trainee = new TraineeSubDetailsDto
                {
                    Id = sd.Trainee.Id,
                    FullName = $"{sd.Trainee.FirstName} {sd.Trainee.LastName}",
                    PhoneNumber = sd.Trainee.PhoneNumber,
                },
                SportName = sd.SportPrice.SportSubscriptionType.Sport.Translations
                    .Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                    ?? sd.SportPrice.SportSubscriptionType.Sport.Name,
                BranchName = sd.SportPrice.Branch.Translations
                    .Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                    ?? sd.SportPrice.Branch.Name,
                SubscriptionTypeName = sd.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString(),
                Price = sd.SportPrice.Price,
                StartDate = sd.StartDate,
                EndDate = sd.EndDate,
                Payment = latestPayment is null ? null : new PaymentSubDetailsDto
                {
                    PaymentNumber = latestPayment.PaymentNumber,
                    PaidDate = latestPayment.PaidDate,
                    BranchName = latestPayment.Branch.Translations
                        .Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                        ?? latestPayment.Branch.Name,
                    PaymentTypeName = latestPayment.PaymentType.Translations
                        .Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                        ?? latestPayment.PaymentType.Name,
                },
                Status = sd.Status,
            };
        }
    }
}

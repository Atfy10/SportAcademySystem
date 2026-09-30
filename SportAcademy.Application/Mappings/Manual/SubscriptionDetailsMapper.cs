using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for the AutoMapper SubscriptionDetails -> SubscriptionDetailsDto
    // mapping (SubscriptionDetailsProfile.cs), used only by the new paginated/search query -
    // the existing unpaginated GetAllSubDetailsQuery keeps using AutoMapper untouched.
    public static class SubscriptionDetailsMapper
    {
        public static SubscriptionDetailsDto ToDto(SubscriptionDetails sd, string lang)
        {
            // Voided payments were recorded in error - never show one as "the" payment.
            var latestPayment = sd.InvoiceLines
                .SelectMany(l => l.Invoice.Allocations)
                .Select(a => a.Payment)
                .Where(p => p.Status != PaymentStatus.Voided)
                .OrderByDescending(p => p.PaidDate)
                .FirstOrDefault();

            var dto = new SubscriptionDetailsDto
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

            ApplyBilling(sd, dto);
            return dto;
        }

        // Fills the derived status and the bill fields. Shared with the AutoMapper profile so
        // every subscription endpoint reports the same status/balance for the same row.
        public static void ApplyBilling(SubscriptionDetails sd, SubscriptionDetailsDto dto)
        {
            var today = SubscriptionBilling.Today;
            dto.Status = SubscriptionBilling.EffectiveStatus(sd, today);

            var invoice = sd.InvoiceLines is null ? null : SubscriptionBilling.CurrentInvoice(sd);
            if (invoice is null) return;

            dto.Price = invoice.GrandTotal;
            dto.InvoiceId = invoice.Id;
            dto.InvoiceBranchId = invoice.BranchId;
            dto.InvoiceNumber = invoice.InvoiceNumber;
            dto.Currency = invoice.Currency;
            dto.AmountPaid = invoice.AmountPaid;
            dto.Balance = invoice.GrandTotal - invoice.AmountPaid;
            dto.BalanceDueDate = dto.Balance > 0 ? invoice.DueDate : null;
            dto.PaymentState = SubscriptionBilling.PaymentState(invoice, today);
        }
    }
}

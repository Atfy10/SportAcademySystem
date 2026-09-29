using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    // What the create-subscription form needs to tell the user exactly what just happened:
    // whether it was paid in full or a balance is still owed (and by when), whether the
    // subscription is running already or starts later, and which receipt to open.
    public record SubscriptionCreatedDto(
        int SubscriptionId,
        string InvoiceNumber,
        string? PaymentNumber,
        decimal Total,
        decimal AmountPaid,
        decimal Balance,
        DateOnly DueDate,
        string Currency,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsUpcoming)
    {
        public static SubscriptionCreatedDto From(SubscriptionCreationResult created)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var sub = created.Subscription;
            var invoice = created.Invoice;
            return new SubscriptionCreatedDto(
                sub.Id,
                invoice.InvoiceNumber,
                created.Payment?.PaymentNumber,
                invoice.GrandTotal,
                invoice.AmountPaid,
                invoice.GrandTotal - invoice.AmountPaid,
                invoice.DueDate,
                invoice.Currency,
                sub.StartDate,
                sub.EndDate,
                sub.Status == SubscriptionStatus.Active && sub.StartDate > today);
        }
    }
}

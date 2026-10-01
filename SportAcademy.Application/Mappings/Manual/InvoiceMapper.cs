using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Mappings.Manual
{
    public static class InvoiceMapper
    {
        public static InvoiceDto ToDto(Invoice invoice) => new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            // Not every payer is a trainee (an event's customer isn't) - fall back to the payer
            // copied onto the invoice.
            invoice.Trainee is null ? invoice.PayerName : $"{invoice.Trainee.FirstName} {invoice.Trainee.LastName}",
            invoice.BranchId,
            invoice.Branch.Name,
            invoice.Currency,
            invoice.SubTotal,
            invoice.DiscountTotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            invoice.AmountPaid,
            invoice.GrandTotal - invoice.AmountPaid,
            invoice.Lines.Select(l => new InvoiceLineDto(
                l.Type.ToString(), l.Description, l.Quantity, l.UnitPrice, l.LineTotal)).ToList()
        );
    }
}

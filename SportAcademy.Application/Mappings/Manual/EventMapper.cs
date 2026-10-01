using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written Event -> EventDto mapping. Expects the entity loaded with its branch and
    // nationality-category translations and its invoice (see IEventRepository), plus the
    // creator/canceller names resolved in one batch by the caller.
    public static class EventMapper
    {
        public static EventDto ToDto(
            Event e, string lang, DateOnly today, IReadOnlyDictionary<Guid, string> userNames)
        {
            var invoice = e.Invoice;
            var invoiceCancelled = invoice?.Status == InvoiceStatus.Cancelled;
            var billed = invoice is null || invoiceCancelled ? 0m : invoice.GrandTotal;
            var paid = invoice?.AmountPaid ?? 0m;
            var balance = invoice is null || invoiceCancelled ? 0m : Math.Max(0m, invoice.GrandTotal - invoice.AmountPaid);

            var branchName = e.Branch?.Translations.FirstOrDefault(t => t.LangCode == lang)?.Name
                ?? e.Branch?.Name ?? string.Empty;
            var startsLocal = TenantCalendar.ToLocal(e.StartsAt);
            var endsLocal = TenantCalendar.ToLocal(e.EndsAt);
            var category = e.EventCustomer?.NationalityCategory;
            var categoryName = category?.Translations.FirstOrDefault(t => t.LangCode == lang)?.Name
                ?? category?.Name ?? string.Empty;

            return new EventDto(
                e.Id,
                e.Title,
                e.BranchId,
                branchName,
                e.EventCustomerId,
                e.EventCustomer?.FullName ?? string.Empty,
                e.EventCustomer?.PhoneNumber ?? string.Empty,
                categoryName,
                e.WithDecorations,
                e.Price,
                e.DecorationFee,
                e.TotalPrice,
                e.Capacity,
                DateTime.SpecifyKind(e.StartsAt, DateTimeKind.Utc),
                DateTime.SpecifyKind(e.EndsAt, DateTimeKind.Utc),
                startsLocal,
                endsLocal,
                EventStatusRules.Resolve(e.IsCancelled, startsLocal, endsLocal, today),
                e.Notes,
                e.InvoiceId,
                invoice?.InvoiceNumber,
                invoice?.Currency ?? "KWD",
                billed,
                paid,
                balance,
                invoice?.DueDate,
                e.IsCancelled || invoice is null || invoiceCancelled ? null : ResolvePaymentState(billed, paid, invoice.DueDate, today),
                e.CreatedByUserId,
                userNames.GetValueOrDefault(e.CreatedByUserId),
                e.CreatedAt,
                e.CancelledAt,
                e.CancelReason,
                e.CancelledByUserId is { } by ? userNames.GetValueOrDefault(by) : null,
                TenantCalendar.ToLocal(EventEntryRules.OpensAt(e.StartsAt)));
        }

        public static IEnumerable<Guid> UserIds(IEnumerable<Event> events)
            => events.SelectMany(e => e.CancelledByUserId is { } by ? new[] { e.CreatedByUserId, by } : [e.CreatedByUserId])
                .Distinct();

        private static EventPaymentState ResolvePaymentState(decimal billed, decimal paid, DateOnly dueDate, DateOnly today)
        {
            if (paid >= billed) return EventPaymentState.Paid;
            if (dueDate < today) return EventPaymentState.Overdue;
            return paid > 0 ? EventPaymentState.PartiallyPaid : EventPaymentState.Unpaid;
        }
    }
}

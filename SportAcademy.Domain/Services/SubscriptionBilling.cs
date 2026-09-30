using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Services
{
    // The single definition of "what state is this subscription in" as the user sees it, shared
    // by every list/detail mapper and mirrored by the repository filters, so the badge, the
    // filter and the stat card can't disagree.
    public static class SubscriptionBilling
    {
        public static DateOnly Today => TenantCalendar.Today;

        public static SubscriptionStatus EffectiveStatus(SubscriptionDetails sd, DateOnly today)
        {
            if (sd.Status != SubscriptionStatus.Active) return sd.Status;
            if (sd.EndDate < today) return SubscriptionStatus.Expired;
            if (sd.StartDate > today) return SubscriptionStatus.Upcoming;
            return SubscriptionStatus.Active;
        }

        // The subscription's bill (a subscription has one invoice; a discount adds a second line
        // on the same invoice). Cancelled invoices don't count - nothing is owed on them.
        public static Invoice? CurrentInvoice(SubscriptionDetails sd)
            => sd.InvoiceLines
                .Select(l => l.Invoice)
                .Where(i => i is not null && i.Status != InvoiceStatus.Cancelled)
                .OrderByDescending(i => i.Id)
                .FirstOrDefault();

        public static SubscriptionPaymentState PaymentState(Invoice invoice, DateOnly today)
        {
            if (invoice.AmountPaid >= invoice.GrandTotal) return SubscriptionPaymentState.Paid;
            if (invoice.DueDate < today) return SubscriptionPaymentState.Overdue;
            return invoice.AmountPaid > 0 ? SubscriptionPaymentState.PartiallyPaid : SubscriptionPaymentState.Unpaid;
        }
    }
}

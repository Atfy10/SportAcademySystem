using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EventExceptions
{
    // Every business-rule rejection for events and event customers, each with its own catalog
    // key ("errors.event.*" in Resources/{en,ar}.json) - same pattern as FinanceRuleException.
    public sealed class EventRuleException : LocalizableException
    {
        private EventRuleException(string key, string fallback, params object[] args)
            : base(key, fallback, args) { }

        public static EventRuleException CustomerPhoneExists(string phone) => new(
            "errors.event.customerPhoneExists",
            $"An event customer with phone number {phone} already exists.",
            phone);

        public static EventRuleException CustomerHasEvents() => new(
            "errors.event.customerHasEvents",
            "This customer has events on record and can't be deleted. Deactivate them instead.");

        public static EventRuleException CustomerInactive() => new(
            "errors.event.customerInactive",
            "This customer is deactivated. Reactivate them before booking a new event.");

        public static EventRuleException AlreadyCancelled() => new(
            "errors.event.alreadyCancelled",
            "This event is already cancelled.");

        public static EventRuleException CancelledReadOnly() => new(
            "errors.event.cancelledReadOnly",
            "A cancelled event can't be changed.");

        public static EventRuleException HasPayments() => new(
            "errors.event.hasPayments",
            "Money has already been collected for this event, so it can't be deleted. Cancel it instead.");

        public static EventRuleException TotalBelowPaid(decimal paid) => new(
            "errors.event.totalBelowPaid",
            $"The new total can't be less than the {paid} already collected for this event.",
            paid);

        public static EventRuleException BranchLocked() => new(
            "errors.event.branchLocked",
            "Money has already been collected for this event at its branch, so the branch can't be changed.");

        public static EventRuleException PaymentExceedsTotal(decimal total) => new(
            "errors.event.paymentExceedsTotal",
            $"The amount collected now can't be more than the event total ({total}).",
            total);

        public static EventRuleException PaymentSharedWithOtherInvoices(string paymentNumber) => new(
            "errors.event.paymentShared",
            $"Payment {paymentNumber} also settles other invoices. Refund it from the Payments page first, then cancel the event.",
            paymentNumber);

        public static EventRuleException CapacityBelowIssued(int issued) => new(
            "errors.event.capacityBelowIssued",
            $"{issued} tickets have already been issued, so the capacity can't be lower than that. Revoke unused tickets first.",
            issued);

        public static EventRuleException TooManyTickets(int remaining) => new(
            "errors.event.tooManyTickets",
            $"Only {remaining} more tickets can be issued for this event - that's its capacity.",
            remaining);

        public static EventRuleException TicketsTerminated() => new(
            "errors.event.ticketsTerminated",
            "This event has ended or was cancelled, so its tickets are closed for good and can't be changed.");

        public static EventRuleException TicketAlreadyUsed() => new(
            "errors.event.ticketAlreadyUsed",
            "This ticket has already been used to get in, so it can't be changed.");

        public static EventRuleException EndingWouldCloseTickets(int issued) => new(
            "errors.event.endingWouldCloseTickets",
            $"This event has {issued} tickets issued. Moving it so it ends in the past would close them for good - check the date and time.",
            issued);

        public static EventRuleException EndedTimesLocked() => new(
            "errors.event.endedTimesLocked",
            "This event has already ended, so its date and time can't be changed - its tickets are closed for good.");

        public static EventRuleException NoActingUser() => new(
            "errors.event.noActingUser",
            "Your session doesn't identify a user, so the event can't be recorded. Please sign in again.");
    }
}

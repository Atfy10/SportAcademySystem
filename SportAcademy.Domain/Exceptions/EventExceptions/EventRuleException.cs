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

        public static EventRuleException CapacityBelowAdmitted(int admitted) => new(
            "errors.event.capacityBelowAdmitted",
            $"{admitted} people have already been let in, so the capacity can't be lower than that.",
            admitted);

        public static EventRuleException NoActingUser() => new(
            "errors.event.noActingUser",
            "Your session doesn't identify a user, so the event can't be recorded. Please sign in again.");
    }
}

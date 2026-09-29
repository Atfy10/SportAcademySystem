using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.PaymentExceptions
{
    // Every business-rule rejection raised by the finance ledger, each with its own catalog key
    // so the user sees a specific, translated reason (not "Something went wrong") - see
    // Resources/{en,ar}.json "errors.finance.*". Built through the factory methods below so the
    // key, fallback English and arguments can't drift apart at call sites.
    public sealed class FinanceRuleException : LocalizableException
    {
        private FinanceRuleException(string key, string fallback, params object[] args)
            : base(key, fallback, args) { }

        public static FinanceRuleException NoAllocations() => new(
            "errors.finance.noAllocations",
            "A payment must be applied to at least one invoice.");

        public static FinanceRuleException AllocationsMismatch(decimal allocated, decimal amount) => new(
            "errors.finance.allocationsMismatch",
            $"The amounts applied to invoices ({allocated}) must add up exactly to the payment amount ({amount}).",
            allocated, amount);

        public static FinanceRuleException AllocationNotPositive() => new(
            "errors.finance.allocationNotPositive",
            "Each amount applied to an invoice must be greater than zero.");

        public static FinanceRuleException InvoiceCancelled(string invoiceNumber) => new(
            "errors.finance.invoiceCancelled",
            $"Invoice {invoiceNumber} is cancelled and can't take a payment.",
            invoiceNumber);

        public static FinanceRuleException Overpayment(string invoiceNumber, decimal outstanding) => new(
            "errors.finance.overpayment",
            $"Invoice {invoiceNumber} only has {outstanding} left to pay.",
            invoiceNumber, outstanding);

        public static FinanceRuleException BranchMismatch(string invoiceNumber) => new(
            "errors.finance.branchMismatch",
            $"Invoice {invoiceNumber} belongs to a different branch than this payment.",
            invoiceNumber);

        public static FinanceRuleException CurrencyMismatch(string invoiceNumber, string invoiceCurrency) => new(
            "errors.finance.currencyMismatch",
            $"Invoice {invoiceNumber} is in {invoiceCurrency}; the payment must be in the same currency.",
            invoiceNumber, invoiceCurrency);

        public static FinanceRuleException RefundOutOfRange(decimal refundable) => new(
            "errors.finance.refundOutOfRange",
            $"The refund must be more than zero and no more than the refundable balance ({refundable}).",
            refundable);

        public static FinanceRuleException AlreadyVoided(string paymentNumber) => new(
            "errors.finance.alreadyVoided",
            $"Payment {paymentNumber} is already voided.",
            paymentNumber);

        public static FinanceRuleException NothingToVoid(string paymentNumber) => new(
            "errors.finance.nothingToVoid",
            $"Payment {paymentNumber} has already been fully refunded - there is nothing left to void.",
            paymentNumber);

        public static FinanceRuleException ReversedPaymentLocked(string paymentNumber) => new(
            "errors.finance.reversedPaymentLocked",
            $"Payment {paymentNumber} has been voided or fully refunded and can no longer be edited.",
            paymentNumber);

        public static FinanceRuleException DueDateInPast() => new(
            "errors.finance.dueDateInPast",
            "The new due date can't be in the past.");

        public static FinanceRuleException SubscriptionHasPayments() => new(
            "errors.finance.subscriptionHasPayments",
            "This subscription has payments on it. Refund or void them from the receipt first, then delete the subscription.");

        public static FinanceRuleException SubscriptionBillingFieldsLocked() => new(
            "errors.finance.subscriptionBillingFieldsLocked",
            "The trainee, sport, branch and plan of a subscription can't be changed after it's billed. Delete it (after refunding) and create a new one instead.");

        public static FinanceRuleException DepositNotLessThanTotal(decimal total) => new(
            "errors.finance.depositNotLessThanTotal",
            $"The deposit must be less than the subscription total ({total}). To pay everything now, untick \"Pay a deposit\".",
            total);
    }
}

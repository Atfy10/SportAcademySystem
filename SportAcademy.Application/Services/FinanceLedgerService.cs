using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.PaymentExceptions;

namespace SportAcademy.Application.Services
{
    // Not transactional on its own: every method here is several SaveChanges calls, so callers
    // wrap them in IUnitOfWork (see UnitOfWorkExtensions.InTransactionAsync) - the command
    // handlers for record/refund/void do, and SubscriptionCreationService already runs inside
    // its own transaction. Invoice and Payment carry rowversion tokens, so two concurrent writes
    // against the same balance fail with a concurrency conflict instead of both succeeding.
    public class FinanceLedgerService : IFinanceLedgerService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBaseRepository<PaymentAllocation, int> _allocationRepository;
        private readonly IBaseRepository<PaymentRefund, int> _refundRepository;
        private readonly IFinancialDocumentNumberGenerator _numberGenerator;

        public FinanceLedgerService(
            IInvoiceRepository invoiceRepository,
            IPaymentRepository paymentRepository,
            IBaseRepository<PaymentAllocation, int> allocationRepository,
            IBaseRepository<PaymentRefund, int> refundRepository,
            IFinancialDocumentNumberGenerator numberGenerator)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _allocationRepository = allocationRepository;
            _refundRepository = refundRepository;
            _numberGenerator = numberGenerator;
        }

        public async Task<Invoice> IssueSubscriptionInvoiceAsync(
            SubscriptionDetails subscription, decimal grossPrice, decimal discountAmount,
            int? discountCodeId, string currency, DateOnly dueDate, CancellationToken ct = default)
        {
            var invoiceNumber = await _numberGenerator.GenerateAsync("INV", ct);

            // A discount can never take the total below zero (a 100% code makes it exactly zero).
            discountAmount = Math.Min(discountAmount, grossPrice);
            var netPrice = grossPrice - discountAmount;

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                // Nothing to collect on a zero-total invoice - it's settled the moment it exists,
                // rather than waiting on a zero-amount payment the ledger (rightly) refuses.
                Status = netPrice == 0 ? InvoiceStatus.Paid : InvoiceStatus.Issued,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = dueDate,
                TraineeId = subscription.TraineeId,
                BranchId = subscription.BranchId,
                Currency = currency,
                SubTotal = grossPrice,
                DiscountTotal = discountAmount,
                TaxTotal = 0,
                GrandTotal = netPrice,
                AmountPaid = 0,
            };

            invoice.Lines.Add(new InvoiceLine
            {
                Type = InvoiceLineType.SubscriptionFee,
                Description = "Subscription fee",
                Quantity = 1,
                UnitPrice = grossPrice,
                DiscountAmount = 0,
                LineTotal = grossPrice,
                SubscriptionDetailsId = subscription.Id,
            });

            // Sum(Lines.LineTotal) == GrandTotal stays a true invariant: grossPrice on the fee
            // line, -discountAmount on this one, nets to grossPrice - discountAmount.
            if (discountAmount > 0)
            {
                invoice.Lines.Add(new InvoiceLine
                {
                    Type = InvoiceLineType.Discount,
                    Description = "Discount code applied",
                    Quantity = 1,
                    UnitPrice = 0,
                    DiscountAmount = discountAmount,
                    LineTotal = -discountAmount,
                    SubscriptionDetailsId = subscription.Id,
                    DiscountCodeId = discountCodeId,
                });
            }

            await _invoiceRepository.AddAsync(invoice, ct);
            return invoice;
        }

        public async Task<Payment> RecordPaymentAsync(RecordPaymentInput input, CancellationToken ct = default)
        {
            if (input.Allocations.Count == 0)
                throw FinanceRuleException.NoAllocations();

            var allocatedTotal = input.Allocations.Sum(a => a.Amount);
            if (allocatedTotal != input.Amount)
                throw FinanceRuleException.AllocationsMismatch(allocatedTotal, input.Amount);

            var invoices = await _invoiceRepository.GetByIdsWithLinesAsync(
                input.Allocations.Select(a => a.InvoiceId), ct);

            // Same invoice listed twice would slip past the per-row overpayment check below.
            var perInvoice = input.Allocations
                .GroupBy(a => a.InvoiceId)
                .Select(g => (InvoiceId: g.Key, Amount: g.Sum(a => a.Amount)))
                .ToList();

            string? currency = string.IsNullOrWhiteSpace(input.Currency) ? null : input.Currency;

            foreach (var (invoiceId, amount) in perInvoice)
            {
                var invoice = invoices.SingleOrDefault(i => i.Id == invoiceId)
                    ?? throw new IdNotFoundException(nameof(Invoice), invoiceId);

                if (invoice.Status is InvoiceStatus.Cancelled)
                    throw FinanceRuleException.InvoiceCancelled(invoice.InvoiceNumber);

                if (amount <= 0)
                    throw FinanceRuleException.AllocationNotPositive();

                if (invoice.AmountPaid + amount > invoice.GrandTotal)
                    throw FinanceRuleException.Overpayment(invoice.InvoiceNumber, invoice.Outstanding);

                // Money is received at a branch and reported per branch - a payment taken at one
                // branch silently settling another branch's invoice would misplace revenue.
                if (invoice.BranchId != input.BranchId)
                    throw FinanceRuleException.BranchMismatch(invoice.InvoiceNumber);

                currency ??= invoice.Currency;
                if (!string.Equals(invoice.Currency, currency, StringComparison.OrdinalIgnoreCase))
                    throw FinanceRuleException.CurrencyMismatch(invoice.InvoiceNumber, invoice.Currency);
            }

            var paymentNumber = await _numberGenerator.GenerateAsync("PAY", ct);

            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                PaymentTypeId = input.PaymentTypeId,
                Status = PaymentStatus.Completed,
                PaidDate = DateTime.UtcNow,
                BranchId = input.BranchId,
                Currency = currency!,
                Amount = input.Amount,
                RefundedAmount = 0,
                RecordedByUserId = input.RecordedByUserId,
                Reference = input.Reference,
                Notes = input.Notes,
            };
            await _paymentRepository.AddAsync(payment, ct);

            foreach (var (invoiceId, amount) in perInvoice)
            {
                var invoice = invoices.Single(i => i.Id == invoiceId);

                await _allocationRepository.AddAsync(new PaymentAllocation
                {
                    PaymentNumber = paymentNumber,
                    InvoiceId = invoiceId,
                    Amount = amount,
                }, ct);

                invoice.AmountPaid += amount;
                invoice.Status = ResolveStatusAfterPayment(invoice);
                await _invoiceRepository.UpdateAsync(invoice, ct);
            }

            return payment;
        }

        public async Task<PaymentRefund> RefundPaymentAsync(
            string paymentNumber, decimal amount, string reason, Guid? actingUserId,
            DateOnly? newDueDate, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetWithAllocationsAsync(paymentNumber, ct)
                ?? throw new PaymentNotFoundException(paymentNumber);

            if (payment.Status is PaymentStatus.Voided)
                throw FinanceRuleException.AlreadyVoided(paymentNumber);

            var refundable = payment.Amount - payment.RefundedAmount;
            if (amount <= 0 || amount > refundable)
                throw FinanceRuleException.RefundOutOfRange(refundable);

            EnsureDueDateNotPast(newDueDate);

            await ReverseAllocationsAsync(payment, amount, newDueDate, ct);

            payment.RefundedAmount += amount;
            payment.Status = payment.RefundedAmount >= payment.Amount
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;

            await _paymentRepository.UpdateAsync(payment, ct);

            return await AddRefundRecordAsync(payment, PaymentRefundKind.Refund, amount, reason, actingUserId, ct);
        }

        public async Task<PaymentRefund> VoidPaymentAsync(
            string paymentNumber, string reason, Guid? actingUserId,
            DateOnly? newDueDate, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetWithAllocationsAsync(paymentNumber, ct)
                ?? throw new PaymentNotFoundException(paymentNumber);

            if (payment.Status is PaymentStatus.Voided)
                throw FinanceRuleException.AlreadyVoided(paymentNumber);

            var remaining = payment.Amount - payment.RefundedAmount;
            if (remaining <= 0)
                throw FinanceRuleException.NothingToVoid(paymentNumber);

            EnsureDueDateNotPast(newDueDate);

            await ReverseAllocationsAsync(payment, remaining, newDueDate, ct);

            payment.RefundedAmount = payment.Amount;
            payment.Status = PaymentStatus.Voided;

            await _paymentRepository.UpdateAsync(payment, ct);

            return await AddRefundRecordAsync(payment, PaymentRefundKind.Void, remaining, reason, actingUserId, ct);
        }

        private async Task<PaymentRefund> AddRefundRecordAsync(
            Payment payment, PaymentRefundKind kind, decimal amount, string reason,
            Guid? actingUserId, CancellationToken ct)
        {
            var refund = new PaymentRefund
            {
                PaymentNumber = payment.PaymentNumber,
                Kind = kind,
                Amount = amount,
                Reason = reason.Trim(),
                RefundedAt = DateTime.UtcNow,
                RefundedByUserId = actingUserId,
            };
            await _refundRepository.AddAsync(refund, ct);
            return refund;
        }

        private static void EnsureDueDateNotPast(DateOnly? newDueDate)
        {
            if (newDueDate is { } due && due < DateOnly.FromDateTime(DateTime.UtcNow))
                throw FinanceRuleException.DueDateInPast();
        }

        // Walks the payment's allocations in order, pulling `amountToReverse` back out of the
        // invoices they were applied to (oldest allocation first) and dropping each invoice's
        // status back down accordingly. Each allocation can only give back what it still holds
        // (Amount - ReversedAmount), so repeated partial refunds across a multi-invoice payment
        // move on to the next invoice instead of driving the first one's AmountPaid negative.
        // Allocation.Amount itself is never changed - it stays the historical record of what was
        // originally applied where.
        private async Task ReverseAllocationsAsync(
            Payment payment, decimal amountToReverse, DateOnly? newDueDate, CancellationToken ct)
        {
            var remaining = amountToReverse;

            foreach (var allocation in payment.Allocations.OrderBy(a => a.Id))
            {
                if (remaining <= 0) break;

                var available = allocation.Amount - allocation.ReversedAmount;
                if (available <= 0) continue;

                var invoice = allocation.Invoice
                    ?? await _invoiceRepository.GetWithLinesAndAllocationsAsync(allocation.InvoiceId, ct)
                    ?? throw new IdNotFoundException(nameof(Invoice), allocation.InvoiceId);

                var applied = Math.Min(remaining, available);
                allocation.ReversedAmount += applied;
                invoice.AmountPaid -= applied;
                invoice.Status = ResolveStatusAfterPayment(invoice);

                if (newDueDate is { } due)
                {
                    invoice.DueDate = due;
                    invoice.OverdueNotifiedOn = null;
                    invoice.DueSoonNotifiedOn = null;
                }

                await _invoiceRepository.UpdateAsync(invoice, ct);

                remaining -= applied;
            }
        }

        private static InvoiceStatus ResolveStatusAfterPayment(Invoice invoice)
        {
            if (invoice.GrandTotal == 0) return InvoiceStatus.Paid;
            if (invoice.AmountPaid <= 0) return InvoiceStatus.Issued;
            return invoice.AmountPaid >= invoice.GrandTotal ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        }
    }
}

using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Services
{
    public class FinanceLedgerService : IFinanceLedgerService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBaseRepository<PaymentAllocation, int> _allocationRepository;
        private readonly IFinancialDocumentNumberGenerator _numberGenerator;

        public FinanceLedgerService(
            IInvoiceRepository invoiceRepository,
            IPaymentRepository paymentRepository,
            IBaseRepository<PaymentAllocation, int> allocationRepository,
            IFinancialDocumentNumberGenerator numberGenerator)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _allocationRepository = allocationRepository;
            _numberGenerator = numberGenerator;
        }

        public async Task<Invoice> IssueSubscriptionInvoiceAsync(
            SubscriptionDetails subscription, decimal price, string currency, CancellationToken ct = default)
        {
            var invoiceNumber = await _numberGenerator.GenerateAsync("INV", ct);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                Status = InvoiceStatus.Issued,
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
                TraineeId = subscription.TraineeId,
                BranchId = subscription.BranchId,
                Currency = currency,
                SubTotal = price,
                DiscountTotal = 0,
                TaxTotal = 0,
                GrandTotal = price,
                AmountPaid = 0,
            };

            invoice.Lines.Add(new InvoiceLine
            {
                Type = InvoiceLineType.SubscriptionFee,
                Description = "Subscription fee",
                Quantity = 1,
                UnitPrice = price,
                DiscountAmount = 0,
                LineTotal = price,
                SubscriptionDetailsId = subscription.Id,
            });

            await _invoiceRepository.AddAsync(invoice, ct);
            return invoice;
        }

        public async Task<Payment> RecordPaymentAsync(RecordPaymentInput input, CancellationToken ct = default)
        {
            if (input.Allocations.Count == 0)
                throw new ArgumentException("A payment must be allocated to at least one invoice.");

            var allocatedTotal = input.Allocations.Sum(a => a.Amount);
            if (allocatedTotal != input.Amount)
                throw new ArgumentException(
                    $"Allocations (total {allocatedTotal}) must sum exactly to the payment amount ({input.Amount}).");

            var invoices = await _invoiceRepository.GetByIdsWithLinesAsync(
                input.Allocations.Select(a => a.InvoiceId), ct);

            foreach (var alloc in input.Allocations)
            {
                var invoice = invoices.SingleOrDefault(i => i.Id == alloc.InvoiceId)
                    ?? throw new IdNotFoundException(nameof(Invoice), alloc.InvoiceId);

                if (invoice.Status is InvoiceStatus.Cancelled)
                    throw new ArgumentException($"Invoice {invoice.InvoiceNumber} is cancelled and cannot accept a payment.");

                if (alloc.Amount <= 0)
                    throw new ArgumentException("Each allocation amount must be greater than zero.");

                if (invoice.AmountPaid + alloc.Amount > invoice.GrandTotal)
                    throw new ArgumentException(
                        $"Allocation of {alloc.Amount} to invoice {invoice.InvoiceNumber} would exceed its outstanding balance.");
            }

            var paymentNumber = await _numberGenerator.GenerateAsync("PAY", ct);

            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                PaymentTypeId = input.PaymentTypeId,
                Status = PaymentStatus.Completed,
                PaidDate = DateTime.UtcNow,
                BranchId = input.BranchId,
                Currency = input.Currency,
                Amount = input.Amount,
                RefundedAmount = 0,
                RecordedByUserId = input.RecordedByUserId,
                Reference = input.Reference,
                Notes = input.Notes,
            };
            await _paymentRepository.AddAsync(payment, ct);

            foreach (var alloc in input.Allocations)
            {
                var invoice = invoices.Single(i => i.Id == alloc.InvoiceId);

                await _allocationRepository.AddAsync(new PaymentAllocation
                {
                    PaymentNumber = paymentNumber,
                    InvoiceId = alloc.InvoiceId,
                    Amount = alloc.Amount,
                }, ct);

                invoice.AmountPaid += alloc.Amount;
                invoice.Status = ResolveStatusAfterPayment(invoice);
                await _invoiceRepository.UpdateAsync(invoice, ct);
            }

            return payment;
        }

        public async Task RefundPaymentAsync(string paymentNumber, decimal amount, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetWithAllocationsAsync(paymentNumber, ct)
                ?? throw new IdNotFoundException(nameof(Payment), paymentNumber);

            var refundable = payment.Amount - payment.RefundedAmount;
            if (amount <= 0 || amount > refundable)
                throw new ArgumentException(
                    $"Refund amount must be between 0 and the refundable balance ({refundable}).");

            await ReverseAllocationsAsync(payment, amount, ct);

            payment.RefundedAmount += amount;
            payment.Status = payment.RefundedAmount >= payment.Amount
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;

            await _paymentRepository.UpdateAsync(payment, ct);
        }

        public async Task VoidPaymentAsync(string paymentNumber, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetWithAllocationsAsync(paymentNumber, ct)
                ?? throw new IdNotFoundException(nameof(Payment), paymentNumber);

            var remaining = payment.Amount - payment.RefundedAmount;
            if (remaining > 0)
                await ReverseAllocationsAsync(payment, remaining, ct);

            payment.RefundedAmount = payment.Amount;
            payment.Status = PaymentStatus.Voided;

            await _paymentRepository.UpdateAsync(payment, ct);
        }

        // Walks the payment's allocations in order, pulling `amountToReverse` back out of the
        // invoices they were applied to (oldest allocation first) and dropping each invoice's
        // status back down accordingly. Allocation rows themselves are left untouched - they
        // stay the historical record of what was originally applied where; Payment.RefundedAmount
        // is what tracks how much of that has since been given back.
        private async Task ReverseAllocationsAsync(Payment payment, decimal amountToReverse, CancellationToken ct)
        {
            var remaining = amountToReverse;

            foreach (var allocation in payment.Allocations.OrderBy(a => a.Id))
            {
                if (remaining <= 0) break;

                var invoice = allocation.Invoice
                    ?? await _invoiceRepository.GetWithLinesAndAllocationsAsync(allocation.InvoiceId, ct)
                    ?? throw new IdNotFoundException(nameof(Invoice), allocation.InvoiceId);

                var applied = Math.Min(remaining, allocation.Amount);
                invoice.AmountPaid -= applied;
                invoice.Status = ResolveStatusAfterPayment(invoice);
                await _invoiceRepository.UpdateAsync(invoice, ct);

                remaining -= applied;
            }
        }

        private static InvoiceStatus ResolveStatusAfterPayment(Invoice invoice)
        {
            if (invoice.AmountPaid <= 0) return InvoiceStatus.Issued;
            return invoice.AmountPaid >= invoice.GrandTotal ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        }
    }
}

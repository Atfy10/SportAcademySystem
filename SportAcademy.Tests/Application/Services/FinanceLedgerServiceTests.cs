using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Application.Services;

// Runs the real ledger against real repositories over an in-memory ApplicationDbContext: the
// bugs this guards against (reversing the same allocation twice, a refund reopening the wrong
// invoice, a zero-total invoice never settling) live in how the ledger and the change tracker
// interact, which a mocked repository can't show.
public class FinanceLedgerServiceTests
{
    private const int BranchId = 1;
    private static readonly Guid TenantId = Guid.NewGuid();

    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public bool AllowCrossTenantWrite { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
        public IDisposable Impersonate(Guid tenantId) => new Noop();
        public IDisposable AllowCrossTenantOperation() => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }

    private sealed class TestBranchAccessProvider : IBranchAccessProvider
    {
        public bool IsRestricted => false;
        public IReadOnlyList<int> AllowedBranchIds => [];
        public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds) { }
    }

    private readonly ApplicationDbContext _ctx;
    private readonly FinanceLedgerService _ledger;
    private int _docCounter;

    public FinanceLedgerServiceTests()
    {
        var tenant = new TestTenantIdProvider();
        tenant.SetTenantId(TenantId);
        // Stamps TenantId on new rows exactly as in production, so the tenant query filter
        // sees what the ledger just wrote.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        _ctx = new ApplicationDbContext(options, tenant, new TestBranchAccessProvider());

        var language = new Mock<ICurrentLanguageProvider>();
        language.SetupGet(l => l.Language).Returns("en");

        var numbers = new Mock<IFinancialDocumentNumberGenerator>();
        numbers.Setup(n => n.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string type, CancellationToken _) => $"{type}-2026-{++_docCounter:D5}");

        // The receipt/refund lookup joins Branch and PaymentType, so they must exist.
        _ctx.Branchs.AddRange(
            new Branch { Id = 1, Name = "Main", City = "Kuwait", Country = "KW", PhoneNumber = "1", IsActive = true, TenantId = TenantId },
            new Branch { Id = 2, Name = "Second", City = "Kuwait", Country = "KW", PhoneNumber = "2", IsActive = true, TenantId = TenantId });
        _ctx.PaymentTypes.Add(new PaymentType { Id = 1, Name = "Cash", IsDefault = true, TenantId = TenantId });
        _ctx.SaveChanges();

        _ledger = new FinanceLedgerService(
            new InvoiceRepository(_ctx),
            new PaymentRepository(_ctx, language.Object),
            new BaseRepository<PaymentAllocation, int>(_ctx),
            new BaseRepository<PaymentRefund, int>(_ctx),
            numbers.Object);
    }

    private async Task<Invoice> SeedInvoiceAsync(decimal total, int branchId = BranchId)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-2026-{++_docCounter:D5}",
            Status = InvoiceStatus.Issued,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            BranchId = branchId,
            Currency = "KWD",
            SubTotal = total,
            GrandTotal = total,
            TenantId = TenantId,
        };
        _ctx.Invoices.Add(invoice);
        await _ctx.SaveChangesAsync();
        return invoice;
    }

    private Task<Payment> PayAsync(params (Invoice Invoice, decimal Amount)[] parts)
        => _ledger.RecordPaymentAsync(new RecordPaymentInput(
            Amount: parts.Sum(p => p.Amount),
            PaymentTypeId: 1,
            BranchId: BranchId,
            Currency: null,
            Reference: null,
            Notes: null,
            RecordedByUserId: null,
            Allocations: parts.Select(p => new PaymentAllocationInput(p.Invoice.Id, p.Amount)).ToList()));

    [Fact]
    public async Task PartialRefund_RefundsOnlyTheRequestedAmount_AndReopensThatMuch()
    {
        var invoice = await SeedInvoiceAsync(100m);
        var payment = await PayAsync((invoice, 100m));

        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 30m, "Missed sessions", null, null);

        payment.RefundedAmount.Should().Be(30m);
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        invoice.AmountPaid.Should().Be(70m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        (await _ctx.PaymentRefunds.SingleAsync()).Amount.Should().Be(30m);
    }

    [Fact]
    public async Task RepeatedPartialRefunds_OnMultiInvoicePayment_MoveToNextInvoice_NeverGoNegative()
    {
        var a = await SeedInvoiceAsync(50m);
        var b = await SeedInvoiceAsync(50m);
        var payment = await PayAsync((a, 50m), (b, 50m));

        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 30m, "r1", null, null);
        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 30m, "r2", null, null);

        // Before the fix the second refund took A from 20 to -10 and left B untouched.
        a.AmountPaid.Should().Be(0m);
        a.Status.Should().Be(InvoiceStatus.Issued);
        b.AmountPaid.Should().Be(40m);
        b.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        payment.RefundedAmount.Should().Be(60m);
    }

    [Fact]
    public async Task Void_AfterPartialRefund_ReversesOnlyTheRemainder()
    {
        var invoice = await SeedInvoiceAsync(100m);
        var payment = await PayAsync((invoice, 100m));

        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 25m, "partial", null, null);
        var voidRecord = await _ledger.VoidPaymentAsync(payment.PaymentNumber, "entered twice", null, null);

        voidRecord.Amount.Should().Be(75m);
        voidRecord.Kind.Should().Be(PaymentRefundKind.Void);
        payment.Status.Should().Be(PaymentStatus.Voided);
        payment.RefundedAmount.Should().Be(100m);
        invoice.AmountPaid.Should().Be(0m);
        (await _ctx.PaymentRefunds.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Refund_MoreThanRefundable_IsRejected()
    {
        var invoice = await SeedInvoiceAsync(100m);
        var payment = await PayAsync((invoice, 100m));
        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 80m, "r1", null, null);

        var act = () => _ledger.RefundPaymentAsync(payment.PaymentNumber, 30m, "r2", null, null);

        (await act.Should().ThrowAsync<FinanceRuleException>())
            .Which.MessageKey.Should().Be("errors.finance.refundOutOfRange");
        invoice.AmountPaid.Should().Be(20m);
    }

    [Fact]
    public async Task Refund_OnVoidedPayment_IsRejected()
    {
        var invoice = await SeedInvoiceAsync(100m);
        var payment = await PayAsync((invoice, 40m));
        await _ledger.VoidPaymentAsync(payment.PaymentNumber, "wrong trainee", null, null);

        var act = () => _ledger.RefundPaymentAsync(payment.PaymentNumber, 10m, "r", null, null);

        (await act.Should().ThrowAsync<FinanceRuleException>())
            .Which.MessageKey.Should().Be("errors.finance.alreadyVoided");
    }

    [Fact]
    public async Task Refund_WithNewDueDate_MovesTheReopenedBalanceDueDate_AndResetsReminders()
    {
        var invoice = await SeedInvoiceAsync(100m);
        invoice.OverdueNotifiedOn = DateOnly.FromDateTime(DateTime.UtcNow);
        var payment = await PayAsync((invoice, 100m));
        var newDue = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);

        await _ledger.RefundPaymentAsync(payment.PaymentNumber, 20m, "r", null, newDue);

        invoice.DueDate.Should().Be(newDue);
        invoice.OverdueNotifiedOn.Should().BeNull();
    }

    [Fact]
    public async Task RecordPayment_AgainstAnotherBranchesInvoice_IsRejected()
    {
        var invoice = await SeedInvoiceAsync(100m, branchId: 2);

        var act = () => PayAsync((invoice, 50m));

        (await act.Should().ThrowAsync<FinanceRuleException>())
            .Which.MessageKey.Should().Be("errors.finance.branchMismatch");
    }

    [Fact]
    public async Task RecordPayment_SameInvoiceListedTwice_CannotOverpay()
    {
        var invoice = await SeedInvoiceAsync(100m);

        var act = () => PayAsync((invoice, 60m), (invoice, 60m));

        (await act.Should().ThrowAsync<FinanceRuleException>())
            .Which.MessageKey.Should().Be("errors.finance.overpayment");
    }

    [Fact]
    public async Task IssueInvoice_WithFullDiscount_IsBornPaid()
    {
        var due = DateOnly.FromDateTime(DateTime.UtcNow);
        var subscription = new SubscriptionDetails { Id = 7, TraineeId = 3, BranchId = BranchId, StartDate = due, EndDate = due.AddDays(30) };

        var invoice = await _ledger.IssueSubscriptionInvoiceAsync(subscription, 80m, 80m, null, "KWD", due);

        invoice.GrandTotal.Should().Be(0m);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task IssueInvoice_UsesTheGivenDueDate()
    {
        var collectDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
        var subscription = new SubscriptionDetails { Id = 8, TraineeId = 3, BranchId = BranchId, StartDate = collectDate, EndDate = collectDate.AddDays(30) };

        var invoice = await _ledger.IssueSubscriptionInvoiceAsync(subscription, 100m, 0m, null, "KWD", collectDate);

        invoice.DueDate.Should().Be(collectDate);
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }
}

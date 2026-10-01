using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Commands.EventCommands.CancelEvent;
using SportAcademy.Application.Commands.EventCommands.RegenerateEventEntryCode;
using SportAcademy.Application.Commands.EventCommands.ScanEventEntry;
using SportAcademy.Application.Commands.EventCommands.CreateEvent;
using SportAcademy.Application.Commands.EventCommands.DeleteEvent;
using SportAcademy.Application.Commands.EventCommands.UpdateEvent;
using SportAcademy.Application.Commands.EventCustomerCommands.DeleteEventCustomer;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Application.Services;

// Runs the event handlers against the real ledger and real repositories over an in-memory
// ApplicationDbContext (same setup as FinanceLedgerServiceTests): what matters here is that a
// booking's money lands in the ledger correctly - invoice lines, payer, deposit, re-pricing,
// cancellation - which only shows up once the change tracker and the ledger actually run.
public class EventBookingTests
{
    private const int BranchId = 1;
    private const int OtherBranchId = 2;
    private const int NationalityCategoryId = 1;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

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

    private sealed class TestUserContextService : IUserContextService
    {
        public Guid? UserId { get; set; }
        public Guid? TenantId => EventBookingTests.TenantId;
        public List<string> Role { get; init; } = [];
        public bool IsAuthenticated => UserId.HasValue;
        public string? IpAddress => null;
        public string? UserAgent => null;
        public Guid? ImpersonationGrantId => null;
    }

    // Stands in for libphonenumber: strips spaces and adds the Kuwait prefix, so "5555 1234"
    // and "+96555551234" normalize to the same number exactly like the real one does.
    private sealed class TestPhoneNormalizer : IPhoneNumberNormalizer
    {
        public Task<string?> NormalizeAsync(string? phone, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phone)) return Task.FromResult(phone);
            var digits = phone.Replace(" ", "");
            return Task.FromResult<string?>(digits.StartsWith('+') ? digits : "+965" + digits);
        }
    }

    private readonly ApplicationDbContext _ctx;
    private readonly FinanceLedgerService _ledger;
    private readonly EventRepository _events;
    private readonly EventCustomerRepository _customers;
    private readonly PaymentRepository _payments;
    private readonly InvoiceRepository _invoices;
    private readonly TestUserContextService _user = new() { UserId = UserId };
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IBranchRepository> _branches = new();
    private readonly Mock<INationalityCategoryRepository> _categories = new();
    private readonly Mock<ITenantSettingsCurrencyReader> _currency = new();
    private readonly EventDetailsLoader _loader;
    private int _docCounter;

    public EventBookingTests()
    {
        var tenant = new TestTenantIdProvider();
        tenant.SetTenantId(TenantId);
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

        _ctx.Branchs.AddRange(
            new Branch { Id = BranchId, Name = "Main", City = "Kuwait", Country = "KW", PhoneNumber = "1", IsActive = true, TenantId = TenantId },
            new Branch { Id = OtherBranchId, Name = "Second", City = "Kuwait", Country = "KW", PhoneNumber = "2", IsActive = true, TenantId = TenantId });
        _ctx.PaymentTypes.Add(new PaymentType { Id = 1, Name = "Cash", IsDefault = true, TenantId = TenantId });
        _ctx.NationalityCategories.Add(new NationalityCategory { Id = NationalityCategoryId, Code = "KW", Name = "Kuwaiti" });
        _ctx.Tenants.Add(new SportAcademy.Domain.Entities.Tenants.Tenant
        {
            Id = TenantId, Name = "aura", DisplayName = "AURA Academy", Email = "a@a.test", Code = "AURA", Slug = "aura",
            Status = TenantStatus.Active,
        });
        _ctx.SaveChanges();

        _invoices = new InvoiceRepository(_ctx);
        _payments = new PaymentRepository(_ctx, language.Object);
        _ledger = new FinanceLedgerService(
            _invoices,
            _payments,
            new BaseRepository<PaymentAllocation, int>(_ctx),
            new BaseRepository<PaymentRefund, int>(_ctx),
            numbers.Object);
        _events = new EventRepository(_ctx);
        _customers = new EventCustomerRepository(_ctx);

        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetDisplayNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [UserId] = "admin" });
        _loader = new EventDetailsLoader(_events, users.Object, language.Object);

        _branches.Setup(b => b.IsExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _categories.Setup(c => c.IsExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _currency.Setup(c => c.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("KWD");
    }

    private CreateEventCommandHandler CreateHandler() => new(
        _events, _customers, _branches.Object, _categories.Object, _ledger, _currency.Object,
        new TestPhoneNormalizer(), _user, _unitOfWork.Object, _loader);

    private static CreateEventCommand Booking(
        string phone = "5555 1234",
        int? customerId = null,
        decimal price = 100m,
        bool withDecorations = true,
        decimal decorationFee = 20m,
        decimal paidNow = 0m,
        int branchId = BranchId)
    {
        var start = DateTime.Today.AddDays(10).AddHours(18);
        return new CreateEventCommand(
            Title: "Birthday party",
            BranchId: branchId,
            EventCustomerId: customerId,
            NewCustomer: customerId is null ? new NewEventCustomerInput("Ahmad Ali", phone, NationalityCategoryId) : null,
            WithDecorations: withDecorations,
            Price: price,
            DecorationFee: decorationFee,
            Capacity: 50,
            StartsAt: start,
            EndsAt: start.AddHours(4),
            Notes: null,
            AmountPaidNow: paidNow,
            PaymentTypeId: paidNow > 0 ? 1 : null,
            BalanceDueDate: null,
            PaymentNote: null);
    }

    [Fact]
    public async Task Create_BillsRentalAndDecorationAsSeparateLines_ToTheCustomer()
    {
        var result = await CreateHandler().Handle(Booking(), default);

        result.IsSuccess.Should().BeTrue();
        var invoice = await _ctx.Invoices.Include(i => i.Lines).SingleAsync();
        invoice.GrandTotal.Should().Be(120m);
        invoice.TraineeId.Should().BeNull();
        invoice.PayerName.Should().Be("Ahmad Ali");
        invoice.PayerPhone.Should().Be("+96555551234");
        invoice.Lines.Should().HaveCount(2);
        invoice.Lines.Single(l => l.Type == InvoiceLineType.EventFee).LineTotal.Should().Be(100m);
        invoice.Lines.Single(l => l.Type == InvoiceLineType.EventDecoration).LineTotal.Should().Be(20m);
        invoice.Lines.Should().OnlyContain(l => l.EventId == result.Data!.Event.Id);

        result.Data!.Event.CreatedByUserId.Should().Be(UserId);
        result.Data.Event.CreatedByName.Should().Be("admin");
        result.Data.Event.Balance.Should().Be(120m);
        result.Data.Event.PaymentState.Should().Be(EventPaymentState.Unpaid);
    }

    // A fixed UTC+3 zone (Kuwait has no daylight saving) so the test doesn't depend on the
    // machine's time-zone database.
    private static readonly TimeZoneInfo Kuwait =
        TimeZoneInfo.CreateCustomTimeZone("Test/Kuwait", TimeSpan.FromHours(3), "Kuwait", "Kuwait");

    [Fact]
    public async Task Create_InAcademyTimeZone_StoresUtc_AndShowsBackWhatWasTyped()
    {
        TenantCalendar.SetTimeZone(Kuwait);
        var typed = new DateTime(2026, 10, 5, 18, 0, 0, DateTimeKind.Unspecified);

        var result = await CreateHandler().Handle(
            Booking() with { StartsAt = typed, EndsAt = typed.AddHours(4) }, default);

        var stored = await _ctx.Events.SingleAsync();
        stored.StartsAt.Should().Be(new DateTime(2026, 10, 5, 15, 0, 0));
        stored.EndsAt.Should().Be(new DateTime(2026, 10, 5, 19, 0, 0));

        var dto = result.Data!.Event;
        dto.StartsAtLocal.Should().Be(typed);
        dto.StartsAtLocal.Kind.Should().Be(DateTimeKind.Unspecified);
        dto.EndsAtLocal.Should().Be(typed.AddHours(4));
        dto.StartsAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Filters_UseTheAcademysCalendarDay()
    {
        TenantCalendar.SetTimeZone(Kuwait);
        // 01:00 on 6 Oct in Kuwait is still 5 Oct in UTC - it must count as a 6 Oct event.
        var typed = new DateTime(2026, 10, 6, 1, 0, 0, DateTimeKind.Unspecified);
        await CreateHandler().Handle(Booking() with { StartsAt = typed, EndsAt = typed.AddHours(2) }, default);

        var (onFifth, _) = await _events.GetPagedAsync(
            new EventListFilter(From: new DateOnly(2026, 10, 5), To: new DateOnly(2026, 10, 5)),
            new DateOnly(2026, 10, 1), SportAcademy.Application.Common.Pagination.PageRequest.Create(1, 10));
        var (onSixth, _) = await _events.GetPagedAsync(
            new EventListFilter(From: new DateOnly(2026, 10, 6), To: new DateOnly(2026, 10, 6)),
            new DateOnly(2026, 10, 1), SportAcademy.Application.Common.Pagination.PageRequest.Create(1, 10));

        onFifth.Should().BeEmpty();
        onSixth.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_WithoutDecorations_IgnoresDecorationFee()
    {
        await CreateHandler().Handle(Booking(withDecorations: false, decorationFee: 20m), default);

        var ev = await _ctx.Events.SingleAsync();
        ev.DecorationFee.Should().Be(0m);
        (await _ctx.Invoices.SingleAsync()).GrandTotal.Should().Be(100m);
        (await _ctx.InvoiceLines.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_SamePhoneTypedDifferently_ReusesTheExistingCustomer()
    {
        await CreateHandler().Handle(Booking(phone: "5555 1234"), default);
        await CreateHandler().Handle(Booking(phone: "+96555551234"), default);

        (await _ctx.EventCustomers.CountAsync()).Should().Be(1);
        (await _ctx.Events.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Create_WithDeposit_RecordsPaymentAndLeavesBalanceOwed()
    {
        var result = await CreateHandler().Handle(Booking(paidNow: 50m), default);

        var invoice = await _ctx.Invoices.SingleAsync();
        invoice.AmountPaid.Should().Be(50m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        (await _ctx.Payments.SingleAsync()).RecordedByUserId.Should().Be(UserId);
        result.Data!.Event.Balance.Should().Be(70m);
        result.Data.Payments.Should().ContainSingle().Which.Amount.Should().Be(50m);

        var (outstanding, _) = await _invoices.GetOutstandingAsync(
            SportAcademy.Application.Common.Pagination.PageRequest.Create(1, 10), null, false);
        outstanding.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_PaymentAboveTotal_IsRejected()
    {
        var act = () => CreateHandler().Handle(Booking(paidNow: 500m), default);

        await act.Should().ThrowAsync<EventRuleException>();
        (await _ctx.Events.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Create_WithNoUserInSession_IsRejected()
    {
        _user.UserId = null;

        var act = () => CreateHandler().Handle(Booking(), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    [Fact]
    public async Task Create_ForDeactivatedCustomer_IsRejected()
    {
        var first = await CreateHandler().Handle(Booking(), default);
        var customer = await _ctx.EventCustomers.SingleAsync();
        customer.IsActive = false;
        await _ctx.SaveChangesAsync();

        var act = () => CreateHandler().Handle(Booking(customerId: first.Data!.Event.CustomerId), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    private UpdateEventCommandHandler UpdateHandler() => new(
        _events, _customers, _branches.Object, _ledger, _currency.Object, _unitOfWork.Object, _loader);

    private static UpdateEventCommand Edit(int id, int customerId, decimal price, bool withDecorations, decimal fee, int branchId = BranchId)
    {
        var start = DateTime.Today.AddDays(10).AddHours(18);
        return new UpdateEventCommand(id, "Birthday party", branchId, customerId, withDecorations, price, fee,
            60, start, start.AddHours(5), null, null);
    }

    [Fact]
    public async Task Update_DroppingDecorations_RepricesTheInvoice()
    {
        var created = (await CreateHandler().Handle(Booking(), default)).Data!.Event;

        var result = await UpdateHandler().Handle(
            Edit(created.Id, created.CustomerId, price: 150m, withDecorations: false, fee: 20m), default);

        result.IsSuccess.Should().BeTrue();
        var invoice = await _ctx.Invoices.Include(i => i.Lines).SingleAsync();
        invoice.GrandTotal.Should().Be(150m);
        invoice.Lines.Should().ContainSingle(l => l.Type == InvoiceLineType.EventFee).Which.LineTotal.Should().Be(150m);
        invoice.Lines.Should().NotContain(l => l.Type == InvoiceLineType.EventDecoration);
    }

    [Fact]
    public async Task Update_TotalBelowAmountCollected_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 100m), default)).Data!.Event;

        var act = () => UpdateHandler().Handle(
            Edit(created.Id, created.CustomerId, price: 50m, withDecorations: false, fee: 0m), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    [Fact]
    public async Task Update_MovingBranchAfterPayment_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 10m), default)).Data!.Event;

        var act = () => UpdateHandler().Handle(
            Edit(created.Id, created.CustomerId, price: 100m, withDecorations: true, fee: 20m, branchId: OtherBranchId), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    private CancelEventCommandHandler CancelHandler() => new(
        _events, _payments, _ledger, _user, _unitOfWork.Object, _loader);

    [Fact]
    public async Task Cancel_RefundingPayments_CancelsInvoiceAndGivesTheMoneyBack()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 50m), default)).Data!.Event;

        var result = await CancelHandler().Handle(
            new CancelEventCommand(created.Id, "Customer called off", EventCancellationMode.RefundPayments), default);

        result.Data!.Event.Status.Should().Be(EventStatus.Cancelled);
        result.Data.Event.Balance.Should().Be(0m);
        var invoice = await _ctx.Invoices.SingleAsync();
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.AmountPaid.Should().Be(0m);
        (await _ctx.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Refunded);
        (await _ctx.PaymentRefunds.SingleAsync()).Amount.Should().Be(50m);

        var (outstanding, _) = await _invoices.GetOutstandingAsync(
            SportAcademy.Application.Common.Pagination.PageRequest.Create(1, 10), null, false);
        outstanding.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancel_KeepingDeposit_ClosesInvoiceAtAmountCollected()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 50m), default)).Data!.Event;

        var result = await CancelHandler().Handle(
            new CancelEventCommand(created.Id, "Non-refundable deposit", EventCancellationMode.KeepPayments), default);

        var invoice = await _ctx.Invoices.Include(i => i.Lines).SingleAsync();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.GrandTotal.Should().Be(50m);
        invoice.Lines.Sum(l => l.LineTotal).Should().Be(50m);
        (await _ctx.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Completed);
        result.Data!.Event.Billed.Should().Be(50m);
        result.Data.Event.Balance.Should().Be(0m);
        result.Data.Event.CancelledByName.Should().Be("admin");
    }

    [Fact]
    public async Task Cancel_KeepingDeposit_RecordsTheWaivedBalanceAsADiscount()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 50m), default)).Data!.Event;

        await CancelHandler().Handle(
            new CancelEventCommand(created.Id, "Non-refundable deposit", EventCancellationMode.KeepPayments), default);

        var invoice = await _ctx.Invoices.SingleAsync();
        (invoice.SubTotal - invoice.DiscountTotal + invoice.TaxTotal).Should().Be(invoice.GrandTotal);
        invoice.DiscountTotal.Should().Be(70m);
    }

    [Fact]
    public async Task Update_CapacityBelowPeopleAlreadyLetIn_IsRejected()
    {
        var (id, token) = await BookRunningEventAsync(capacity: 5);
        await ScanAsync(token, "device-aaaa1");
        await ScanAsync(token, "device-bbbb2");
        var ev = await _ctx.Events.SingleAsync(e => e.Id == id);
        _ctx.ChangeTracker.Clear();

        var start = DateTime.UtcNow.AddMinutes(-5);
        var act = () => UpdateHandler().Handle(new UpdateEventCommand(
            id, "Birthday party", BranchId, ev.EventCustomerId, true, 100m, 20m, Capacity: 1,
            start, start.AddHours(3), null, null), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    [Fact]
    public async Task Cancel_Twice_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(), default)).Data!.Event;
        await CancelHandler().Handle(new CancelEventCommand(created.Id, "x", EventCancellationMode.RefundPayments), default);

        var act = () => CancelHandler().Handle(new CancelEventCommand(created.Id, "x", EventCancellationMode.RefundPayments), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    [Fact]
    public async Task Delete_WithPaymentHistory_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 10m), default)).Data!.Event;

        var act = () => new DeleteEventCommandHandler(_events, _invoices, _unitOfWork.Object)
            .Handle(new DeleteEventCommand(created.Id), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }

    [Fact]
    public async Task Delete_Unpaid_RemovesEventAndItsBill()
    {
        var created = (await CreateHandler().Handle(Booking(), default)).Data!.Event;

        await new DeleteEventCommandHandler(_events, _invoices, _unitOfWork.Object)
            .Handle(new DeleteEventCommand(created.Id), default);

        (await _ctx.Events.CountAsync()).Should().Be(0);
        (await _ctx.Invoices.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task FinancialStatement_NamesAnEventPaymentAfterTheEvent()
    {
        await CreateHandler().Handle(Booking(paidNow: 50m), default);

        var income = await new FinancialStatementReader(_ctx).GetIncomeAsync(null, null, null, "en", limit: 100);

        var line = income.Should().ContainSingle().Subject;
        line.Kind.Should().Be(IncomeKind.Event);
        line.EventTitle.Should().Be("Birthday party");
        line.PayerName.Should().Be("Ahmad Ali");
        line.Amount.Should().Be(50m);
    }

    // ── Entry QR code ────────────────────────────────────────────────────────

    private ScanEventEntryCommandHandler ScanHandler()
    {
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(t => t.IsFeatureEnabledAsync(TenantId, "event-management", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var clock = new Mock<ITenantClock>();
        clock.Setup(c => c.GetTimeZoneAsync(It.IsAny<CancellationToken>())).ReturnsAsync((TimeZoneInfo?)null);
        var language = new Mock<ICurrentLanguageProvider>();
        language.SetupGet(l => l.Language).Returns("en");
        return new ScanEventEntryCommandHandler(
            new EventEntryStore(_ctx), tenants.Object, new TestTenantIdProvider(), clock.Object, language.Object);
    }

    /// Books an event running from `startsInMinutes` from now, for `hours`, with `capacity` places.
    private async Task<(int Id, string Token)> BookRunningEventAsync(int startsInMinutes = -5, int capacity = 50, double hours = 3)
    {
        var start = DateTime.UtcNow.AddMinutes(startsInMinutes);
        var created = (await CreateHandler().Handle(
            Booking() with { StartsAt = start, EndsAt = start.AddHours(hours), Capacity = capacity }, default)).Data!.Event;
        return (created.Id, created.EntryToken);
    }

    private async Task<EventEntryResult> ScanAsync(string token, string device)
        => (await ScanHandler().Handle(new ScanEventEntryCommand(token, device), default)).Data!.Result;

    [Fact]
    public async Task Scan_FirstTime_AdmitsAndNumbersThePerson()
    {
        var (id, token) = await BookRunningEventAsync();

        var result = (await ScanHandler().Handle(new ScanEventEntryCommand(token, "device-aaaa1"), default)).Data!;

        result.Result.Should().Be(EventEntryResult.Admitted);
        result.AdmissionNumber.Should().Be(1);
        result.AdmittedCount.Should().Be(1);
        result.Capacity.Should().Be(50);
        result.AcademyName.Should().Be("AURA Academy");
        (await _ctx.Events.SingleAsync(e => e.Id == id)).AdmittedCount.Should().Be(1);
    }

    [Fact]
    public async Task Scan_SamePhoneAgain_IsAlreadyAdmitted_AndTakesNoNewPlace()
    {
        var (id, token) = await BookRunningEventAsync();
        await ScanAsync(token, "device-aaaa1");

        var again = (await ScanHandler().Handle(new ScanEventEntryCommand(token, "device-aaaa1"), default)).Data!;

        again.Result.Should().Be(EventEntryResult.AlreadyAdmitted);
        again.AdmissionNumber.Should().Be(1);
        (await _ctx.Events.SingleAsync(e => e.Id == id)).AdmittedCount.Should().Be(1);
    }

    [Fact]
    public async Task Scan_WhenCapacityIsReached_RefusesEntryAsFull()
    {
        var (id, token) = await BookRunningEventAsync(capacity: 2);

        (await ScanAsync(token, "device-aaaa1")).Should().Be(EventEntryResult.Admitted);
        (await ScanAsync(token, "device-bbbb2")).Should().Be(EventEntryResult.Admitted);
        (await ScanAsync(token, "device-cccc3")).Should().Be(EventEntryResult.Full);
        // Someone already inside who rescans is still recognised, not refused.
        (await ScanAsync(token, "device-aaaa1")).Should().Be(EventEntryResult.AlreadyAdmitted);

        (await _ctx.Events.SingleAsync(e => e.Id == id)).AdmittedCount.Should().Be(2);
        (await _ctx.EventAdmissions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Scan_OpensFifteenMinutesBeforeTheStart()
    {
        var (_, soon) = await BookRunningEventAsync(startsInMinutes: 10);
        var (_, later) = await BookRunningEventAsync(startsInMinutes: 20);

        (await ScanAsync(soon, "device-aaaa1")).Should().Be(EventEntryResult.Admitted);
        (await ScanAsync(later, "device-aaaa1")).Should().Be(EventEntryResult.NotYetOpen);
    }

    [Fact]
    public async Task Scan_AfterTheEnd_IsRefused()
    {
        var (_, token) = await BookRunningEventAsync(startsInMinutes: -240, hours: 2);

        (await ScanAsync(token, "device-aaaa1")).Should().Be(EventEntryResult.Ended);
    }

    [Fact]
    public async Task Scan_CancelledEvent_IsRefused()
    {
        var (id, token) = await BookRunningEventAsync();
        await CancelHandler().Handle(new CancelEventCommand(id, "Called off", EventCancellationMode.RefundPayments), default);

        (await ScanAsync(token, "device-aaaa1")).Should().Be(EventEntryResult.Cancelled);
    }

    [Fact]
    public async Task Scan_UnknownOrReplacedCode_IsInvalid_AndRevealsNothing()
    {
        var (id, oldToken) = await BookRunningEventAsync();
        await new RegenerateEventEntryCodeCommandHandler(_events, _loader).Handle(new RegenerateEventEntryCodeCommand(id), default);

        var result = (await ScanHandler().Handle(new ScanEventEntryCommand(oldToken, "device-aaaa1"), default)).Data!;

        result.Result.Should().Be(EventEntryResult.Invalid);
        result.EventTitle.Should().BeNull();
        result.AcademyName.Should().BeNull();
        (await ScanAsync("0123456789abcdef0123456789abcdef", "device-aaaa1")).Should().Be(EventEntryResult.Invalid);
    }

    [Fact]
    public async Task Create_GivesEachEventItsOwnEntryCode()
    {
        var (_, first) = await BookRunningEventAsync();
        var (_, second) = await BookRunningEventAsync();

        first.Should().MatchRegex("^[0-9a-f]{32}$");
        second.Should().NotBe(first);
    }

    [Fact]
    public async Task DeleteCustomer_WithEvents_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(), default)).Data!.Event;

        var act = () => new DeleteEventCustomerCommandHandler(_customers)
            .Handle(new DeleteEventCustomerCommand(created.CustomerId), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }
}

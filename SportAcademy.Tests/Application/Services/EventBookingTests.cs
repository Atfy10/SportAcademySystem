using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SportAcademy.Application.Commands.EventCommands.CancelEvent;
using SportAcademy.Application.Commands.EventCommands.CreateEvent;
using SportAcademy.Application.Commands.EventCommands.DeleteEvent;
using SportAcademy.Application.Commands.EventCommands.UpdateEvent;
using SportAcademy.Application.Commands.EventCustomerCommands.DeleteEventCustomer;
using SportAcademy.Application.Commands.EventTicketCommands.AdmitEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.IssueEventTickets;
using SportAcademy.Application.Commands.EventTicketCommands.ReissueEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.RevokeEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.UpdateEventTicket;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Queries.EventTicketQueries.CheckEventTicket;
using SportAcademy.Application.Queries.EventTicketQueries.GetCheckInEvents;
using SportAcademy.Application.Queries.EventTicketQueries.GetEventTickets;
using SportAcademy.Application.Queries.EventTicketQueries.GetPublicEventTicket;
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
        _events, _customers, _branches.Object, _ledger, _currency.Object, _unitOfWork.Object, _loader, new EventTicketStore(_ctx));

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
    public async Task Update_CapacityBelowTicketsIssued_IsRejected()
    {
        var id = await BookRunningEventAsync(capacity: 5);
        await IssueAsync(id, 2);
        var ev = await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == id);

        var act = () => UpdateHandler().Handle(new UpdateEventCommand(
            id, "Birthday party", BranchId, ev.EventCustomerId, true, 100m, 20m, Capacity: 1,
            ev.StartsAt, ev.EndsAt, null, null), default);

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

    // ── Tickets & door check-in ──────────────────────────────────────────────

    private EventTicketStore TicketStore() => new(_ctx);

    private static Mock<IUserRepository> UserNames()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetDisplayNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [UserId] = "admin" });
        return users;
    }

    private static ICurrentLanguageProvider English()
    {
        var language = new Mock<ICurrentLanguageProvider>();
        language.SetupGet(l => l.Language).Returns("en");
        return language.Object;
    }

    private EventTicketCheckService CheckService() => new(TicketStore(), UserNames().Object, English());

    private async Task<List<EventTicketDto>> IssueAsync(int eventId, int count, string? guestName = null)
        => (await new IssueEventTicketsCommandHandler(_events, TicketStore())
            .Handle(new IssueEventTicketsCommand(eventId, count, guestName), default)).Data!;

    private async Task<EventTicketCheckDto> CheckAsync(string? code, int? eventId = null, int? number = null)
        => (await new CheckEventTicketQueryHandler(CheckService())
            .Handle(new CheckEventTicketQuery(code, eventId, number), default)).Data!;

    private async Task<EventTicketCheckDto> AdmitAsync(string? code, int? eventId = null, int? number = null)
        => (await new AdmitEventTicketCommandHandler(CheckService(), TicketStore(), _user)
            .Handle(new AdmitEventTicketCommand(code, eventId, number), default)).Data!;

    private GetPublicEventTicketQueryHandler PublicHandler()
    {
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(t => t.IsFeatureEnabledAsync(TenantId, "event-management", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var clock = new Mock<ITenantClock>();
        clock.Setup(c => c.GetTimeZoneAsync(It.IsAny<CancellationToken>())).ReturnsAsync((TimeZoneInfo?)null);
        return new GetPublicEventTicketQueryHandler(TicketStore(), tenants.Object, new TestTenantIdProvider(), clock.Object, English());
    }

    private async Task<PublicEventTicketDto> PublicTicketAsync(string token)
        => (await PublicHandler().Handle(new GetPublicEventTicketQuery(token), default)).Data!;

    /// Books an event running from `startsInMinutes` from now, for `hours`, with `capacity` places.
    private async Task<int> BookRunningEventAsync(int startsInMinutes = -5, int capacity = 50, double hours = 3)
    {
        var start = DateTime.UtcNow.AddMinutes(startsInMinutes);
        var created = (await CreateHandler().Handle(
            Booking() with { StartsAt = start, EndsAt = start.AddHours(hours), Capacity = capacity }, default)).Data!.Event;
        return created.Id;
    }

    // Ends the event a minute ago, as if time had passed since its tickets were issued.
    private async Task EndEventAsync(int eventId)
    {
        var ev = await _ctx.Events.SingleAsync(e => e.Id == eventId);
        ev.StartsAt = DateTime.UtcNow.AddHours(-3);
        ev.EndsAt = DateTime.UtcNow.AddMinutes(-1);
        await _ctx.SaveChangesAsync();
        _ctx.ChangeTracker.Clear();
    }

    [Fact]
    public async Task IssueTickets_GivesEachARandomNumberAndItsOwnCode_UpToTheCapacity()
    {
        var id = await BookRunningEventAsync(capacity: 3);

        var first = await IssueAsync(id, 2);
        first.Should().OnlyContain(t => t.Number >= EventEntryRules.TicketNumberMin && t.Number <= EventEntryRules.TicketNumberMax);
        first[0].Number.Should().NotBe(first[1].Number);
        first.Should().OnlyContain(t => System.Text.RegularExpressions.Regex.IsMatch(t.Token, "^[0-9a-f]{32}$"));
        first[0].Token.Should().NotBe(first[1].Token);

        var tooMany = () => IssueAsync(id, 2);
        await tooMany.Should().ThrowAsync<EventRuleException>();

        var named = await IssueAsync(id, 1, "  Sara  ");
        named.Single().GuestName.Should().Be("Sara");
        named.Single().Number.Should().NotBe(first[0].Number).And.NotBe(first[1].Number);
        (await _ctx.EventTickets.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task TicketNumbers_AreNotSequential()
    {
        var id = await BookRunningEventAsync(capacity: 50);

        var numbers = (await IssueAsync(id, 50)).Select(t => t.Number).ToList();

        numbers.Should().OnlyHaveUniqueItems();
        // 50 random 6-digit numbers coming out as a run of consecutive ones is practically impossible.
        numbers.Order().Zip(numbers.Order().Skip(1), (a, b) => b - a).Should().Contain(gap => gap > 1);
    }

    [Fact]
    public void NewTicketNumber_NeverRepeatsATakenNumber()
    {
        // Every number but one is taken: the only possible answer is the free one.
        var taken = Enumerable.Range(EventEntryRules.TicketNumberMin, EventEntryRules.TicketNumberMax - EventEntryRules.TicketNumberMin + 1)
            .Where(n => n != 555_555)
            .ToHashSet();

        EventEntryRules.NewTicketNumber(taken).Should().Be(555_555);
        taken.Should().Contain(555_555);
    }

    [Fact]
    public async Task RevokedTicket_FreesItsPlace_AndItsCodeStopsWorking()
    {
        var id = await BookRunningEventAsync(capacity: 3);
        var tickets = await IssueAsync(id, 3);

        await new RevokeEventTicketCommandHandler(TicketStore()).Handle(new RevokeEventTicketCommand(tickets[1].Id), default);

        (await CheckAsync(tickets[1].Token)).Result.Should().Be(EventTicketCheckResult.Invalid);
        (await CheckAsync(null, id, tickets[1].Number)).Result.Should().Be(EventTicketCheckResult.Invalid);
        (await IssueAsync(id, 1)).Should().ContainSingle();
    }

    [Fact]
    public async Task Check_ValidTicket_SaysSo_ButLetsNobodyIn()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 1, "Sara")).Single();

        // The QR code holds the ticket's whole link; the scanner sends it as read.
        var result = await CheckAsync($"https://app.example.com/ticket/{ticket.Token}?utm=x");

        result.Result.Should().Be(EventTicketCheckResult.Valid);
        result.Number.Should().Be(ticket.Number);
        result.GuestName.Should().Be("Sara");
        result.EventTitle.Should().Be("Birthday party");
        result.AdmittedCount.Should().Be(0);
        (await _ctx.EventTickets.SingleAsync()).AdmittedAt.Should().BeNull();
    }

    [Fact]
    public async Task Admit_LetsTheTicketInOnce_ThenItIsAlreadyUsed()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 2))[0];

        var admitted = await AdmitAsync(ticket.Token);
        admitted.Result.Should().Be(EventTicketCheckResult.Admitted);
        admitted.AdmittedCount.Should().Be(1);
        admitted.AdmittedByName.Should().Be("admin");
        admitted.AdmittedAtLocal.Should().NotBeNull();

        var again = await AdmitAsync(ticket.Token);
        again.Result.Should().Be(EventTicketCheckResult.AlreadyUsed);
        again.AdmittedByName.Should().Be("admin");
        (await CheckAsync(ticket.Token)).Result.Should().Be(EventTicketCheckResult.AlreadyUsed);

        var stored = await _ctx.EventTickets.SingleAsync(t => t.Id == ticket.Id);
        stored.AdmittedByUserId.Should().Be(UserId);
        (await _ctx.EventTickets.CountAsync(t => t.AdmittedAt != null)).Should().Be(1);
    }

    [Fact]
    public async Task Admit_ByEventAndNumber_WhenTheCodeCantBeScanned()
    {
        var id = await BookRunningEventAsync();
        var tickets = await IssueAsync(id, 3);
        var number = tickets[1].Number;
        var notIssued = Enumerable.Range(EventEntryRules.TicketNumberMin, 10).First(n => tickets.All(t => t.Number != n));

        (await AdmitAsync(null, id, number)).Result.Should().Be(EventTicketCheckResult.Admitted);
        (await CheckAsync(null, id, number)).Result.Should().Be(EventTicketCheckResult.AlreadyUsed);
        (await CheckAsync(null, id, notIssued)).Result.Should().Be(EventTicketCheckResult.Invalid);
    }

    [Fact]
    public async Task Admit_OpensFifteenMinutesBeforeTheStart()
    {
        var soon = (await IssueAsync(await BookRunningEventAsync(startsInMinutes: 10), 1)).Single();
        var later = (await IssueAsync(await BookRunningEventAsync(startsInMinutes: 20), 1)).Single();

        (await AdmitAsync(soon.Token)).Result.Should().Be(EventTicketCheckResult.Admitted);
        (await AdmitAsync(later.Token)).Result.Should().Be(EventTicketCheckResult.NotYetOpen);
        (await _ctx.EventTickets.SingleAsync(t => t.Id == later.Id)).AdmittedAt.Should().BeNull();
    }

    [Fact]
    public async Task UnknownOrReissuedCode_IsInvalid_AndRevealsNothing()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 1)).Single();

        var reissued = (await new ReissueEventTicketCommandHandler(TicketStore())
            .Handle(new ReissueEventTicketCommand(ticket.Id), default)).Data!;

        reissued.Number.Should().NotBe(ticket.Number);
        reissued.Token.Should().NotBe(ticket.Token);
        // The old ticket can't get in by its number at the manual fallback either.
        (await CheckAsync(null, id, ticket.Number)).Result.Should().Be(EventTicketCheckResult.Invalid);
        var old = await AdmitAsync(ticket.Token);
        old.Result.Should().Be(EventTicketCheckResult.Invalid);
        old.EventTitle.Should().BeNull();
        (await CheckAsync("0123456789abcdef0123456789abcdef")).Result.Should().Be(EventTicketCheckResult.Invalid);
        (await CheckAsync("not a ticket")).Result.Should().Be(EventTicketCheckResult.Invalid);
        (await AdmitAsync(reissued.Token)).Result.Should().Be(EventTicketCheckResult.Admitted);
    }

    [Fact]
    public async Task UsedTicket_CantBeRevokedReissuedOrRenamed()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 1, "Sara")).Single();
        await AdmitAsync(ticket.Token);

        var revoke = () => new RevokeEventTicketCommandHandler(TicketStore()).Handle(new RevokeEventTicketCommand(ticket.Id), default);
        var reissue = () => new ReissueEventTicketCommandHandler(TicketStore()).Handle(new ReissueEventTicketCommand(ticket.Id), default);
        var rename = () => new UpdateEventTicketCommandHandler(TicketStore(), UserNames().Object)
            .Handle(new UpdateEventTicketCommand(ticket.Id, "Someone else"), default);

        await revoke.Should().ThrowAsync<EventRuleException>();
        await reissue.Should().ThrowAsync<EventRuleException>();
        await rename.Should().ThrowAsync<EventRuleException>();
        (await _ctx.EventTickets.SingleAsync()).GuestName.Should().Be("Sara");
    }

    [Fact]
    public async Task RunningEventWithTickets_CantBeMovedToEndInThePast()
    {
        var id = await BookRunningEventAsync();
        await IssueAsync(id, 1);
        var ev = await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == id);
        _ctx.ChangeTracker.Clear();

        // A mistyped year would close every ticket for good.
        var lastYear = DateTime.UtcNow.AddYears(-1);
        var act = () => UpdateHandler().Handle(new UpdateEventCommand(
            id, "Birthday party", BranchId, ev.EventCustomerId, true, 100m, 20m, 50,
            lastYear, lastYear.AddHours(3), null, null), default);

        await act.Should().ThrowAsync<EventRuleException>();
        (await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == id)).EndsAt.Should().Be(ev.EndsAt);
    }

    [Fact]
    public async Task CancelledEvent_TerminatesItsTickets()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 1)).Single();
        await CancelHandler().Handle(new CancelEventCommand(id, "Called off", EventCancellationMode.RefundPayments), default);

        (await AdmitAsync(ticket.Token)).Result.Should().Be(EventTicketCheckResult.Cancelled);
        var page = await PublicTicketAsync(ticket.Token);
        page.Result.Should().Be(EventTicketCheckResult.Cancelled);
        page.Number.Should().BeNull();
    }

    [Fact]
    public async Task EndedEvent_TerminatesEveryTicket_ForGood()
    {
        var id = await BookRunningEventAsync();
        var tickets = await IssueAsync(id, 2);
        await AdmitAsync(tickets[0].Token);
        await EndEventAsync(id);

        // The door refuses it, used or not.
        (await AdmitAsync(tickets[1].Token)).Result.Should().Be(EventTicketCheckResult.Ended);
        (await CheckAsync(tickets[0].Token)).Result.Should().Be(EventTicketCheckResult.Ended);
        (await _ctx.EventTickets.SingleAsync(t => t.Id == tickets[1].Id)).AdmittedAt.Should().BeNull();

        // The guest's page stops showing the ticket at all.
        var page = await PublicTicketAsync(tickets[1].Token);
        page.Result.Should().Be(EventTicketCheckResult.Ended);
        page.AcademyName.Should().Be("AURA Academy");
        page.Number.Should().BeNull();
        page.GuestName.Should().BeNull();
        page.StartsAtLocal.Should().BeNull();

        // Nothing about the tickets can be changed any more.
        var issue = () => IssueAsync(id, 1);
        var rename = () => new UpdateEventTicketCommandHandler(TicketStore(), UserNames().Object)
            .Handle(new UpdateEventTicketCommand(tickets[1].Id, "Late guest"), default);
        var reissue = () => new ReissueEventTicketCommandHandler(TicketStore()).Handle(new ReissueEventTicketCommand(tickets[1].Id), default);
        var revoke = () => new RevokeEventTicketCommandHandler(TicketStore()).Handle(new RevokeEventTicketCommand(tickets[1].Id), default);
        await issue.Should().ThrowAsync<EventRuleException>();
        await rename.Should().ThrowAsync<EventRuleException>();
        await reissue.Should().ThrowAsync<EventRuleException>();
        await revoke.Should().ThrowAsync<EventRuleException>();

        // The record of who came stays.
        var list = (await new GetEventTicketsQueryHandler(_events, TicketStore(), UserNames().Object)
            .Handle(new GetEventTicketsQuery(id, EventTicketFilter.All, null, PageRequest.Create(1, 10)), default)).Data!;
        list.Terminated.Should().BeTrue();
        list.Issued.Should().Be(2);
        list.Admitted.Should().Be(1);
        list.Tickets.Items.Single(t => t.Number == tickets[0].Number).AdmittedByName.Should().Be("admin");
    }

    [Fact]
    public async Task EndedEvent_CantBeMovedLater_ToReviveItsTickets()
    {
        var id = await BookRunningEventAsync();
        await IssueAsync(id, 1);
        await EndEventAsync(id);
        var ev = await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == id);

        var later = DateTime.UtcNow.AddHours(1);
        var move = () => UpdateHandler().Handle(new UpdateEventCommand(
            id, "Birthday party", BranchId, ev.EventCustomerId, true, 100m, 20m, 50,
            later, later.AddHours(3), null, null), default);
        await move.Should().ThrowAsync<EventRuleException>();

        // Correcting the price of an ended event (same times) is still fine.
        var reprice = await UpdateHandler().Handle(new UpdateEventCommand(
            id, "Birthday party", BranchId, ev.EventCustomerId, true, 90m, 20m, 50,
            ev.StartsAt, ev.EndsAt, null, null), default);
        reprice.IsSuccess.Should().BeTrue();
        reprice.Data!.Event.Price.Should().Be(90m);
    }

    // Ended on an earlier academy day - shown as Completed.
    private async Task CompleteEventAsync(int eventId)
    {
        var ev = await _ctx.Events.SingleAsync(e => e.Id == eventId);
        ev.StartsAt = DateTime.UtcNow.AddDays(-2);
        ev.EndsAt = ev.StartsAt.AddHours(3);
        await _ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task CompletedEvent_CantBeEdited()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 10m), default)).Data!.Event;
        await CompleteEventAsync(created.Id);
        var ev = await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == created.Id);

        // Not even a change that leaves the date and time alone.
        var act = () => UpdateHandler().Handle(new UpdateEventCommand(
            ev.Id, "Renamed", BranchId, ev.EventCustomerId, true, 90m, 20m, 50,
            ev.StartsAt, ev.EndsAt, "late note", null), default);

        await act.Should().ThrowAsync<EventRuleException>()
            .Where(e => e.MessageKey == "errors.event.completedReadOnly");
        (await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == ev.Id)).Price.Should().Be(100m);
    }

    [Fact]
    public async Task CompletedEvent_CantBeCancelled()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 50m), default)).Data!.Event;
        await CompleteEventAsync(created.Id);

        var act = () => CancelHandler().Handle(
            new CancelEventCommand(created.Id, "Too late", EventCancellationMode.RefundPayments), default);

        await act.Should().ThrowAsync<EventRuleException>()
            .Where(e => e.MessageKey == "errors.event.completedReadOnly");
        (await _ctx.Events.AsNoTracking().SingleAsync(e => e.Id == created.Id)).IsCancelled.Should().BeFalse();
        (await _ctx.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task CompletedEvent_BalanceCanStillBeCollected()
    {
        var created = (await CreateHandler().Handle(Booking(paidNow: 50m), default)).Data!.Event;
        await CompleteEventAsync(created.Id);
        var invoice = await _ctx.Invoices.SingleAsync();

        await _ledger.RecordPaymentAsync(new RecordPaymentInput(
            Amount: 70m, PaymentTypeId: 1, BranchId: BranchId, Currency: null, Reference: null,
            Notes: null, RecordedByUserId: UserId,
            Allocations: [new PaymentAllocationInput(invoice.Id, 70m)]));

        var details = await _loader.LoadAsync(created.Id, default);
        details.Event.Status.Should().Be(EventStatus.Completed);
        details.Event.Balance.Should().Be(0m);
        (await _ctx.Invoices.SingleAsync()).Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public async Task PublicTicketPage_ShowsTheTicket_AndNeverLetsAnyoneIn()
    {
        var id = await BookRunningEventAsync();
        var ticket = (await IssueAsync(id, 1, "Sara")).Single();

        var page = await PublicTicketAsync(ticket.Token);

        page.Result.Should().Be(EventTicketCheckResult.Valid);
        page.Number.Should().Be(ticket.Number);
        page.GuestName.Should().Be("Sara");
        page.EventTitle.Should().Be("Birthday party");
        page.AcademyName.Should().Be("AURA Academy");
        (await _ctx.EventTickets.SingleAsync()).AdmittedAt.Should().BeNull();

        await AdmitAsync(ticket.Token);
        var used = await PublicTicketAsync(ticket.Token);
        used.Result.Should().Be(EventTicketCheckResult.AlreadyUsed);
        used.UsedAtLocal.Should().NotBeNull();

        var unknown = await PublicTicketAsync("0123456789abcdef0123456789abcdef");
        unknown.Result.Should().Be(EventTicketCheckResult.Invalid);
        unknown.AcademyName.Should().BeNull();
    }

    [Fact]
    public async Task CheckInEvents_ListsTodaysEventsThatHaveNotEnded()
    {
        var running = await BookRunningEventAsync();
        var issued = await IssueAsync(running, 2);
        await AdmitAsync(issued[0].Token);
        var ended = await BookRunningEventAsync();
        await EndEventAsync(ended);

        var events = (await new GetCheckInEventsQueryHandler(TicketStore(), English())
            .Handle(new GetCheckInEventsQuery(), default)).Data!;

        var only = events.Should().ContainSingle().Subject;
        only.Id.Should().Be(running);
        only.IsOpen.Should().BeTrue();
        only.Issued.Should().Be(2);
        only.Admitted.Should().Be(1);
    }

    [Theory]
    [InlineData("0123456789ABCDEF0123456789abcdef", "0123456789abcdef0123456789abcdef")]
    [InlineData(" https://x.test/ticket/0123456789abcdef0123456789abcdef/ ", "0123456789abcdef0123456789abcdef")]
    [InlineData("https://x.test/ticket/0123456789abcdef0123456789abcdef#top", "0123456789abcdef0123456789abcdef")]
    [InlineData("https://x.test/ticket/short", null)]
    [InlineData("", null)]
    public void TicketCode_IsReadFromTheLinkOrTheBareToken(string code, string? expected)
        => EventTicketCheckService.ParseToken(code).Should().Be(expected);

    [Fact]
    public async Task DeleteCustomer_WithEvents_IsRejected()
    {
        var created = (await CreateHandler().Handle(Booking(), default)).Data!.Event;

        var act = () => new DeleteEventCustomerCommandHandler(_customers)
            .Handle(new DeleteEventCustomerCommand(created.CustomerId), default);

        await act.Should().ThrowAsync<EventRuleException>();
    }
}

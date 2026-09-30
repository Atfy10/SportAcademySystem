using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Mappings.EnrollmentProfile;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Projections;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// The enrollments list/detail show the LATER of the subscription's end date and the enrollment's
// own ExpiryDate (an upcoming subscription enrolled into the current group extends ExpiryDate
// before the hand-over). That expression has to translate to SQL - ToQueryString() on the real
// SqlServer provider proves it without connecting, as in BranchQueryFilterTranslationTests.
public class EnrollmentEndDateProjectionTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; } = Guid.NewGuid();
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

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=NeverConnected;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new ApplicationDbContext(options, new TestTenantIdProvider(), new TestBranchAccessProvider());
    }

    private static MapperConfiguration Config() =>
        new(cfg => cfg.AddProfile<EnrollmentMappingProfile>(), LoggerFactory.Create(_ => { }));

    [Fact]
    public void EnrollmentCardDto_EndDateProjection_TranslatesToSql()
    {
        using var ctx = CreateContext();

        var sql = ctx.Set<Enrollment>().ProjectTo<EnrollmentCardDto>(Config()).ToQueryString();

        sql.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EnrollmentDetailDto_EndDateProjection_TranslatesToSql()
    {
        using var ctx = CreateContext();

        var sql = ctx.Set<Enrollment>().ProjectTo<EnrollmentDetailDto>(Config()).ToQueryString();

        sql.Should().NotBeNullOrWhiteSpace();
    }

    // The group list/card counts only enrollments that haven't ended - so trainees moved out of a
    // group that went private stop counting toward it.
    [Fact]
    public void TraineeGroupListAndCardProjections_TranslateToSql()
    {
        using var ctx = CreateContext();

        var list = ctx.Set<TraineeGroup>().Select(TraineeGroupProjections.ToListDto("en")).ToQueryString();
        var card = ctx.Set<TraineeGroup>().Select(TraineeGroupProjections.ToCardDto("en")).ToQueryString();

        list.Should().NotBeNullOrWhiteSpace();
        card.Should().NotBeNullOrWhiteSpace();
    }

    // The groups list and search narrow by public/private together with the hours filter. As with
    // the dropdown test below, only the missing database may fail, never the translation.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GroupList_TypeFilter_TranslatesToSql(bool viaSearch)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=NeverConnected;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;")
            .Options;
        using var ctx = new ApplicationDbContext(options, new TestTenantIdProvider(), new TestBranchAccessProvider());
        var repo = new TraineeGroupRepository(
            ctx, new Moq.Mock<IMapper>().Object, new Moq.Mock<ICurrentLanguageProvider>().Object);
        var page = SportAcademy.Application.Common.Pagination.PageRequest.Create(1, 9);
        var from = new TimeOnly(8, 0);
        var to = new TimeOnly(14, 0);

        Func<Task> act = viaSearch
            ? async () => await repo.SearchAsync("foot", page, from, to, TraineeGroupType.Private)
            : async () => await repo.GetAllAsCardAsync(page, from, to, TraineeGroupType.Private);

        var thrown = await Record.ExceptionAsync(act);
        (thrown?.Message ?? "").Should().NotContain("could not be translated");
    }

    // An upcoming subscription already applied to the trainee's current enrollment must drop out of
    // the enrollment dropdown. The query is translated before any connection is attempted, so the
    // missing database is the only acceptable failure; a translation problem says "could not be
    // translated" (EF wraps the connection failure in its own InvalidOperationException, so the
    // type alone can't tell them apart).
    [Fact]
    public async Task SubscriptionDropdown_AppliedUpcomingFilter_TranslatesToSql()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=NeverConnected;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;")
            .Options;
        using var ctx = new ApplicationDbContext(options, new TestTenantIdProvider(), new TestBranchAccessProvider());
        var repo = new SubscriptionDetailsRepository(
            ctx, new Moq.Mock<IMapper>().Object, new Moq.Mock<ICurrentLanguageProvider>().Object);

        var act = () => repo.GetActiveForTraineeDropdownAsync(11);

        var thrown = await Record.ExceptionAsync(act);
        (thrown?.Message ?? "").Should().NotContain("could not be translated");
    }
}

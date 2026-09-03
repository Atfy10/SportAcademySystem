using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Tests.Integration;

// Regression coverage for the branch-scoped EF global query filter (tenant + soft-delete +
// branch, ANDed together in ApplicationDbContext.OnModelCreating). Uses the real SqlServer
// provider (never connects - ToQueryString() only needs to *translate* the LINQ query to SQL
// text, not execute it) to catch a SQL-Server-specific translation failure - e.g. from the
// navigated-branch-scoped Expression tree - that the InMemory provider would silently paper over.
public class BranchQueryFilterTranslationTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
    }

    private sealed class TestBranchAccessProvider : IBranchAccessProvider
    {
        public bool IsRestricted { get; private set; }
        public IReadOnlyList<int> AllowedBranchIds { get; private set; } = [];
        public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds)
        {
            IsRestricted = isRestricted;
            AllowedBranchIds = allowedBranchIds;
        }
    }

    private static ApplicationDbContext CreateSqlServerContext(bool restricted, int[]? allowed = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=NeverConnected;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        var tenantProvider = new TestTenantIdProvider();
        tenantProvider.SetTenantId(Guid.NewGuid());

        var branchProvider = new TestBranchAccessProvider();
        branchProvider.SetBranchAccess(restricted, allowed ?? []);

        return new ApplicationDbContext(options, tenantProvider, branchProvider);
    }

    private static void AssertTranslates<T>(IQueryable<T> query, string label)
    {
        string sql;
        try
        {
            sql = query.ToQueryString();
        }
        catch (Exception ex)
        {
            Assert.Fail($"{label} FAILED TO TRANSLATE: {ex.GetType().Name}: {ex.Message}");
            return;
        }
        // Surface the SQL in the test output either way, for manual inspection.
        Assert.False(string.IsNullOrWhiteSpace(sql), $"{label} produced empty SQL");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectlyBranchScoped_Entities_Translate(bool restricted)
    {
        using var ctx = CreateSqlServerContext(restricted, [1, 2]);

        AssertTranslates(ctx.Set<Trainee>(), "Trainee");
        AssertTranslates(ctx.Set<TraineeGroup>(), "TraineeGroup");
        AssertTranslates(ctx.Set<Employee>(), "Employee");
        AssertTranslates(ctx.Set<SubscriptionDetails>(), "SubscriptionDetails");
        AssertTranslates(ctx.Set<Payment>(), "Payment");
        AssertTranslates(ctx.Set<SportPrice>(), "SportPrice");
        AssertTranslates(ctx.Set<SportBranch>(), "SportBranch");
        AssertTranslates(ctx.Set<SportAcademy.Domain.Entities.Finance.Invoice>(), "Invoice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NavigatedBranchScoped_Entities_Translate(bool restricted)
    {
        using var ctx = CreateSqlServerContext(restricted, [1, 2]);

        AssertTranslates(ctx.Set<Enrollment>(), "Enrollment");
        AssertTranslates(ctx.Set<Attendance>(), "Attendance");
        AssertTranslates(ctx.Set<SessionOccurrence>(), "SessionOccurrence");
        AssertTranslates(ctx.Set<Coach>(), "Coach");
        AssertTranslates(ctx.Set<ExcuseRequest>(), "ExcuseRequest");
    }

    [Fact]
    public void Unrelated_Entities_StillTranslate()
    {
        using var ctx = CreateSqlServerContext(false);

        AssertTranslates(ctx.Set<Branch>(), "Branch");
        AssertTranslates(ctx.Set<Sport>(), "Sport");
        AssertTranslates(ctx.Set<Family>(), "Family");
    }
}

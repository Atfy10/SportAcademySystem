using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// Regression coverage for a real bug found by manual QA: TenantRepository.GetResourceUsageAsync
// queries Branch/Sport/AppUser/Trainee/Invitation - all ITenantScoped - by an explicit tenantId
// PARAMETER, but every caller that actually matters (ChangeTenantPlanCommandHandler,
// UpdatePlanLimitsCommandHandler, SetTenantLimitOverrideCommandHandler, the reopen/bypass
// commands) runs under the SuperAdmin's OWN ambient tenant, not the target tenant's. Without
// IgnoreQueryFilters(), the global filter silently ANDs in "AND TenantId == CallerTenantId" on
// top of the explicit filter, matching zero rows for any tenant other than the caller's own -
// so usage was permanently computed as 0 and the downgrade lock could never engage from the only
// path that ever triggers it. A mocked unit test (see LimitReconciliationServiceTests-style
// coverage elsewhere) cannot catch this class of bug at all, since it never exercises a real
// DbContext's global query filters - this is exactly why this suite uses a real (in-memory)
// ApplicationDbContext instead, mirroring TenantIsolationTests' own reasoning for existing.
public class GetResourceUsageAsyncCrossTenantTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public bool AllowCrossTenantWrite { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;

        public IDisposable Impersonate(Guid tenantId)
        {
            var previous = TenantId;
            TenantId = tenantId;
            return new RestoreScope(() => TenantId = previous);
        }

        public IDisposable AllowCrossTenantOperation()
        {
            var previous = AllowCrossTenantWrite;
            AllowCrossTenantWrite = true;
            return new RestoreScope(() => AllowCrossTenantWrite = previous);
        }

        private sealed class RestoreScope(Action restore) : IDisposable
        {
            public void Dispose() => restore();
        }
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

    private static ApplicationDbContext CreateContext(Guid? ambientTenantId, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var provider = new TestTenantIdProvider();
        provider.SetTenantId(ambientTenantId);

        return new ApplicationDbContext(options, provider, new TestBranchAccessProvider());
    }

    [Fact]
    public async Task GetResourceUsageAsync_CalledUnderADifferentAmbientTenant_StillCountsTheTargetTenantsRows()
    {
        // The exact shape of the real bug: seed data as tenantA, then call
        // GetResourceUsageAsync(tenantA) from a context whose ambient tenant is tenantB - the
        // SuperAdmin's own System tenant in production, an arbitrary other tenant here.
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Set<Branch>().Add(new Branch
            {
                Name = "Main Branch", City = "City", Country = "C", PhoneNumber = "0",
                IsActive = true, TenantId = tenantA,
            });
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "owner", Email = "owner@test.com",
                IsBanned = false, TenantId = tenantA,
            });
            seedCtx.Set<Invitation>().Add(new Invitation
            {
                Id = Guid.NewGuid(), Email = "invitee@test.com", Status = InvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1), TenantId = tenantA,
            });
            await seedCtx.SaveChangesAsync();
        }

        // Ambient tenant is B, not A - simulating a SuperAdmin-triggered command (e.g.
        // ChangeTenantPlanCommandHandler) evaluating tenant A's usage from the Platform console.
        using var callerCtx = CreateContext(tenantB, dbName);
        var repository = new TenantRepository(callerCtx);

        var usage = await repository.GetResourceUsageAsync(tenantA, CancellationToken.None);

        // Before the fix, every one of these was 0 regardless of what was seeded.
        usage[LimitedResources.Branches].Should().Be(1);
        // The pending invitation counts as a consumed seat alongside the one real user.
        usage[LimitedResources.Users].Should().Be(2);
    }

    [Fact]
    public async Task GetResourceUsageAsync_DoesNotCountSoftDeletedOrBannedRows()
    {
        // IgnoreQueryFilters() disables the ENTIRE combined filter on AppUser/Trainee (tenant +
        // soft-delete are one HasQueryFilter, not two independent ones) - this locks in that the
        // fix re-adds the soft-delete/ban exclusion explicitly rather than accidentally starting
        // to count deleted or banned rows as "used".
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "active-user", Email = "a@test.com",
                IsBanned = false, IsDeleted = false, TenantId = tenantA,
            });
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "banned-user", Email = "b@test.com",
                IsBanned = true, IsDeleted = false, TenantId = tenantA,
            });
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "deleted-user", Email = "c@test.com",
                IsBanned = false, IsDeleted = true, TenantId = tenantA,
            });
            await seedCtx.SaveChangesAsync();
        }

        using var callerCtx = CreateContext(Guid.NewGuid(), dbName);
        var repository = new TenantRepository(callerCtx);

        var usage = await repository.GetResourceUsageAsync(tenantA, CancellationToken.None);

        usage[LimitedResources.Users].Should().Be(1);
    }
}

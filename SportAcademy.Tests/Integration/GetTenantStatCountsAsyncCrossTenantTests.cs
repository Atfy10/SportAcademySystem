using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// Regression coverage for a real bug reported from production: the Platform tenant-details
// page's stat cards (Users/Branches/Sports) always showed 0 for every tenant.
// TenantRepository.GetUserCountByTenantAsync/GetBranchCountByTenantAsync/
// GetSportCountByTenantAsync query by an explicit tenantId PARAMETER, but the only caller
// (GetTenantDetailsQueryHandler, via the Platform console) always runs under the SuperAdmin's
// OWN ambient tenant (the System tenant), never the tenantId parameter's tenant. Without
// IgnoreQueryFilters(), the global filter silently ANDs in "AND TenantId == SystemTenantId" on
// top of the explicit filter, matching zero rows for any real tenant - exact same shape as the
// already-fixed GetResourceUsageAsync bug (see GetResourceUsageAsyncCrossTenantTests), just
// missed for these three. A mocked unit test cannot catch this class of bug at all, since it
// never exercises a real DbContext's global query filters - hence the real (in-memory)
// ApplicationDbContext here, same as that sibling suite.
public class GetTenantStatCountsAsyncCrossTenantTests
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
    public async Task StatCounts_CalledUnderADifferentAmbientTenant_StillCountTheTargetTenantsRows()
    {
        // The exact shape of the real bug: seed data as tenantA, then call the count methods for
        // tenantA from a context whose ambient tenant is tenantB - the SuperAdmin's own System
        // tenant in production, an arbitrary other tenant here.
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
            seedCtx.Set<Sport>().Add(new Sport { Name = "Swimming", TenantId = tenantA });
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "owner", Email = "owner@test.com",
                IsBanned = false, TenantId = tenantA,
            });
            await seedCtx.SaveChangesAsync();
        }

        // Ambient tenant is B, not A - simulating the SuperAdmin viewing tenant A's details page
        // from the Platform console.
        using var callerCtx = CreateContext(tenantB, dbName);
        var repository = new TenantRepository(callerCtx);

        // Before the fix, every one of these was 0 regardless of what was seeded.
        (await repository.GetUserCountByTenantAsync(tenantA, CancellationToken.None)).Should().Be(1);
        (await repository.GetBranchCountByTenantAsync(tenantA, CancellationToken.None)).Should().Be(1);
        (await repository.GetSportCountByTenantAsync(tenantA, CancellationToken.None)).Should().Be(1);
    }

    [Fact]
    public async Task GetUserCountByTenantAsync_DoesNotCountSoftDeletedUsers()
    {
        // IgnoreQueryFilters() disables the ENTIRE combined filter on AppUser (tenant +
        // soft-delete are one HasQueryFilter, not two independent ones) - this locks in that the
        // fix re-adds the soft-delete exclusion explicitly rather than accidentally starting to
        // count deleted rows as "belonging to this tenant".
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "active-user", Email = "a@test.com",
                IsDeleted = false, TenantId = tenantA,
            });
            seedCtx.Set<AppUser>().Add(new AppUser
            {
                Id = Guid.NewGuid(), UserName = "deleted-user", Email = "b@test.com",
                IsDeleted = true, TenantId = tenantA,
            });
            await seedCtx.SaveChangesAsync();
        }

        using var callerCtx = CreateContext(Guid.NewGuid(), dbName);
        var repository = new TenantRepository(callerCtx);

        (await repository.GetUserCountByTenantAsync(tenantA, CancellationToken.None)).Should().Be(1);
    }

    [Fact]
    public async Task GetBranchCountByTenantAsync_CountsInactiveBranchesToo()
    {
        // Unlike GetResourceUsageAsync (which only counts what's currently "used", i.e. active),
        // this is the raw "how many branches does this tenant have" stat - it must count every
        // row regardless of IsActive.
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Set<Branch>().Add(new Branch
            {
                Name = "Active Branch", City = "City", Country = "C", PhoneNumber = "0",
                IsActive = true, TenantId = tenantA,
            });
            seedCtx.Set<Branch>().Add(new Branch
            {
                Name = "Inactive Branch", City = "City", Country = "C", PhoneNumber = "1",
                IsActive = false, TenantId = tenantA,
            });
            await seedCtx.SaveChangesAsync();
        }

        using var callerCtx = CreateContext(Guid.NewGuid(), dbName);
        var repository = new TenantRepository(callerCtx);

        (await repository.GetBranchCountByTenantAsync(tenantA, CancellationToken.None)).Should().Be(2);
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Integration;

// Regression coverage for a real bug found by manual QA: an UPGRADE (Basic -> Professional,
// Basic -> Enterprise, Pro -> Enterprise) was incorrectly opening a PendingLimitSelection
// reconciliation, exactly like a downgrade would. Root cause: ChangeTenantPlanCommandHandler
// sets the new SubscriptionPlanId on the tracked TenantSubscription entity, then calls
// LimitReconciliationService.EvaluateAsync/RecheckAsync BEFORE its own SaveChangesAsync -
// EvaluateAsync reaches TenantRepository.GetCurrentPlanIdAsync via IEffectiveLimitService, which
// used a .Select(s => s.SubscriptionPlanId) projection. A projected scalar query always executes
// against the database and ignores the change tracker, even for a row that's already tracked
// with a pending edit - so the limit check ran against the OLD plan's caps regardless of which
// plan the tenant was actually being moved to. For an upgrade, if usage already exceeded the OLD
// (lower) plan's caps, this spuriously locked the tenant even though the NEW (higher) plan had
// ample headroom. A mocked handler test (ChangeTenantPlanOpensLimitReconciliationTests) cannot
// catch this class of bug at all, since it stubs ILimitReconciliationService entirely and never
// exercises the real GetCurrentPlanIdAsync query against a real change tracker - this is exactly
// why this suite uses a real (in-memory) ApplicationDbContext instead, mirroring
// GetResourceUsageAsyncCrossTenantTests' own reasoning for existing.
public class GetCurrentPlanIdAsyncTrackedChangeTests
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
    public async Task GetCurrentPlanIdAsync_ReturnsPendingUnsavedPlanChange_NotTheStalePersistedValue()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantId, dbName))
        {
            seedCtx.Set<TenantSubscription>().Add(new TenantSubscription
            {
                TenantId = tenantId,
                SubscriptionPlanId = 1, // "Basic"
                StartsAt = DateTime.UtcNow,
                EndsAt = DateTime.UtcNow.AddYears(1),
            });
            await seedCtx.SaveChangesAsync();
        }

        // Same DbContext throughout, mirroring ChangeTenantPlanCommandHandler's own scope: it
        // loads the tracked TenantSubscription, mutates SubscriptionPlanId in memory, then calls
        // EvaluateAsync/RecheckAsync (which reach GetCurrentPlanIdAsync) BEFORE its own
        // SaveChangesAsync.
        using var ctx = CreateContext(tenantId, dbName);
        var repository = new TenantRepository(ctx);

        var subscription = await ctx.TenantSubscriptions.FirstAsync(s => s.TenantId == tenantId);
        subscription.SubscriptionPlanId = 3; // "Enterprise" - an upgrade, not yet saved

        var planId = await repository.GetCurrentPlanIdAsync(tenantId, CancellationToken.None);

        // Before the fix, this returned 1 (the stale, still-persisted value) regardless of the
        // in-memory change, causing the limit check to run against the wrong plan's caps.
        planId.Should().Be(3);
    }
}

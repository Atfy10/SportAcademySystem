using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;

namespace SportAcademy.Tests.Infrastructure.Interceptors;

public class AuditImmutabilityInterceptorTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;

        public IDisposable Impersonate(Guid tenantId)
        {
            var previous = TenantId;
            TenantId = tenantId;
            return new RestoreScope(() => TenantId = previous);
        }

        private sealed class RestoreScope(Action restore) : IDisposable
        {
            public void Dispose() => restore();
        }
    }

    private sealed class TestBranchAccessProvider : IBranchAccessProvider
    {
        public bool IsRestricted => false;
        public IReadOnlyList<int> AllowedBranchIds { get; } = [];
        public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds) { }
    }

    private static ApplicationDbContext CreateContext(string dbName)
    {
        var interceptor = new AuditImmutabilityInterceptor();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(interceptor)
            .Options;

        return new ApplicationDbContext(options, new TestTenantIdProvider(), new TestBranchAccessProvider());
    }

    private static TenantAuditEvent CreateEvent(Guid tenantId) => new()
    {
        TenantId = tenantId,
        EventType = "tenant.archived",
        Description = "Tenant archived successfully.",
        Outcome = AuditOutcome.Succeeded,
        PerformedByUserId = Guid.NewGuid(),
        PerformedBy = "Test SuperAdmin",
    };

    [Fact]
    public async Task SaveChangesAsync_InsertingANewEvent_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);

        context.Set<TenantAuditEvent>().Add(CreateEvent(Guid.NewGuid()));
        var rows = await context.SaveChangesAsync();

        rows.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifyingAnExistingEvent_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        var auditEvent = CreateEvent(Guid.NewGuid());

        await using (var seedContext = CreateContext(dbName))
        {
            seedContext.Set<TenantAuditEvent>().Add(auditEvent);
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(dbName);
        var tracked = await context.Set<TenantAuditEvent>().FirstAsync(e => e.Id == auditEvent.Id);
        tracked.Description = "Edited after the fact";

        var act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChangesAsync_DeletingAnExistingEvent_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        var auditEvent = CreateEvent(Guid.NewGuid());

        await using (var seedContext = CreateContext(dbName))
        {
            seedContext.Set<TenantAuditEvent>().Add(auditEvent);
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(dbName);
        var tracked = await context.Set<TenantAuditEvent>().FirstAsync(e => e.Id == auditEvent.Id);
        context.Set<TenantAuditEvent>().Remove(tracked);

        var act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }
}

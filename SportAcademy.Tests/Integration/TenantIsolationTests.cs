using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;

namespace SportAcademy.Tests.Integration;

public class TenantIsolationTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
    }

    private sealed class TestUserContextService : IUserContextService
    {
        public Guid? UserId { get; init; }
        public Guid? TenantId { get; init; }
        public List<string> Role { get; init; } = [];
        public bool IsAuthenticated => UserId.HasValue;
    }

    private static ApplicationDbContext CreateContext(Guid? tenantId, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var provider = new TestTenantIdProvider();
        if (tenantId.HasValue)
            provider.SetTenantId(tenantId.Value);

        return new ApplicationDbContext(options, provider);
    }

    private static ApplicationDbContext CreateContextWithInterceptor(Guid? tenantId, string dbName)
    {
        var tenantIdProvider = new TestTenantIdProvider();
        tenantIdProvider.SetTenantId(tenantId);
        var interceptor = new TenantSaveChangesInterceptor(tenantIdProvider);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(interceptor)
            .Options;

        var provider = new TestTenantIdProvider();
        if (tenantId.HasValue)
            provider.SetTenantId(tenantId.Value);

        return new ApplicationDbContext(options, provider);
    }

    private static Branch CreateBranch(string name, Guid? tenantId = null)
    {
        return new Branch
        {
            Name = name,
            City = "City",
            Country = "C",
            PhoneNumber = "0",
            CoX = "0",
            CoY = "0",
            TenantId = tenantId ?? Guid.Empty
        };
    }

    [Fact]
    public async Task GlobalQueryFilter_WithNullTenantId_ReturnsEmpty()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();

        using (var seedCtx = CreateContext(tenantA, dbName))
        {
            seedCtx.Set<Branch>().Add(CreateBranch("Branch A", tenantA));
            await seedCtx.SaveChangesAsync();
        }

        using (var ctxA = CreateContext(tenantA, dbName))
        {
            var branches = await ctxA.Set<Branch>().ToListAsync();
            Assert.Single(branches);
        }

        using (var nullCtx = CreateContext(null, dbName))
        {
            var branches = await nullCtx.Set<Branch>().ToListAsync();
            Assert.Empty(branches);
        }
    }

    [Fact]
    public async Task GlobalQueryFilter_TenantA_DoesNotSeeTenantB_Data()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var seedCtx = CreateContext(null, dbName))
        {
            seedCtx.Set<Branch>().Add(CreateBranch("Branch A1", tenantA));
            seedCtx.Set<Branch>().Add(CreateBranch("Branch A2", tenantA));
            seedCtx.Set<Branch>().Add(CreateBranch("Branch B1", tenantB));
            seedCtx.Set<Branch>().Add(CreateBranch("Branch B2", tenantB));
            seedCtx.Set<Branch>().Add(CreateBranch("Branch B3", tenantB));
            await seedCtx.SaveChangesAsync();
        }

        using (var ctxA = CreateContext(tenantA, dbName))
        {
            var branchesA = await ctxA.Set<Branch>().ToListAsync();
            Assert.Equal(2, branchesA.Count);
            Assert.All(branchesA, b => Assert.Equal(tenantA, b.TenantId));
        }

        using (var ctxB = CreateContext(tenantB, dbName))
        {
            var branchesB = await ctxB.Set<Branch>().ToListAsync();
            Assert.Equal(3, branchesB.Count);
            Assert.All(branchesB, b => Assert.Equal(tenantB, b.TenantId));
        }
    }

    [Fact]
    public async Task SaveChangesInterceptor_AutoSetsTenantId()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        int branchId;

        using (var ctx = CreateContextWithInterceptor(tenantA, dbName))
        {
            ctx.Database.EnsureCreated();

            var branch = CreateBranch("AutoTenant Branch");
            ctx.Set<Branch>().Add(branch);
            await ctx.SaveChangesAsync();
            branchId = branch.Id;

            Assert.Equal(tenantA, branch.TenantId);
        }

        using (var verifyCtx = CreateContext(null, dbName))
        {
            var saved = await verifyCtx.Set<Branch>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == branchId);

            Assert.NotNull(saved);
            Assert.Equal(tenantA, saved.TenantId);
        }
    }

    [Fact]
    public async Task SaveChangesInterceptor_CannotChangeTenantId_OnExistingEntity()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        int branchId;

        using (var ctx = CreateContextWithInterceptor(tenantA, dbName))
        {
            ctx.Database.EnsureCreated();

            var branch = CreateBranch("Fixed Tenant Branch", tenantA);
            ctx.Set<Branch>().Add(branch);
            await ctx.SaveChangesAsync();
            branchId = branch.Id;
        }

        using (var ctx = CreateContextWithInterceptor(tenantA, dbName))
        {
            var branch = await ctx.Set<Branch>()
                .IgnoreQueryFilters()
                .FirstAsync(b => b.Id == branchId);

            branch.TenantId = tenantB;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.SaveChangesAsync());
            Assert.Contains("TenantId", ex.Message);
        }
    }
}

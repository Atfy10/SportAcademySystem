using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Tests.Infrastructure.Repositories;

// Regression coverage for a critical bug found while smoke-testing the live app: /auth/refresh
// is [AllowAnonymous], so ITenantIdProvider.TenantId is null for that request. The global
// tenant query filter (e.TenantId == CurrentTenantId) then silently nulled out the Included
// AppUser navigation on every refresh-token lookup - since a null CurrentTenantId can never
// match a real TenantId - making JwtTokenService.ValidateAndRefreshTokenAsync reject every
// single refresh attempt as "invalid". Confirmed live: every call to /api/auth/refresh
// returned 401 before this fix. RefreshTokenRepository now uses IgnoreQueryFilters() to
// resolve identity before any tenant context exists, with explicit IsDeleted/IsBanned checks
// in JwtTokenService taking over what the (also-bypassed) soft-delete filter would have
// enforced.
public class RefreshTokenRepositoryTests
{
    private sealed class TestTenantIdProvider : ITenantIdProvider
    {
        public Guid? TenantId { get; private set; }
        public void SetTenantId(Guid? tenantId) => TenantId = tenantId;
    }

    private static ApplicationDbContext CreateContext(Guid? tenantId, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var provider = new TestTenantIdProvider();
        provider.SetTenantId(tenantId);

        return new ApplicationDbContext(options, provider);
    }

    [Fact]
    public async Task GetByTokenHashAsync_WithNoTenantContext_StillResolvesTheUser()
    {
        // Mirrors the real /auth/refresh request: anonymous, so no tenant_id claim exists and
        // ITenantIdProvider.TenantId is null at query time.
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserName = "refresh-flow-user",
            Email = "refresh-flow-user@test.com",
        };

        await using (var seedCtx = CreateContext(tenantId, dbName))
        {
            seedCtx.Users.Add(user);
            seedCtx.Set<RefreshToken>().Add(new RefreshToken
            {
                TokenHash = "hash-under-test",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
            });
            await seedCtx.SaveChangesAsync();
        }

        // The context used for the lookup has NO tenant set - exactly like an anonymous
        // /auth/refresh request.
        await using var anonymousCtx = CreateContext(null, dbName);
        var repository = new RefreshTokenRepository(anonymousCtx);

        var storedToken = await repository.GetByTokenHashAsync("hash-under-test");

        Assert.NotNull(storedToken);
        // FIXED: without IgnoreQueryFilters(), the tenant filter (TenantId == null) would
        // silently exclude the AppUser row from the Include, leaving this null.
        Assert.NotNull(storedToken!.User);
        Assert.Equal(user.Id, storedToken.User.Id);
    }

    [Fact]
    public async Task GetByTokenHashAsync_WithNoTenantContext_StillResolvesDeletedOrBannedUsers()
    {
        // The repository intentionally returns a deleted/banned user too now (it bypasses all
        // filters, not just the tenant one) - JwtTokenService is responsible for rejecting
        // them explicitly. This test documents that split of responsibility.
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        var bannedUser = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserName = "banned-user",
            Email = "banned-user@test.com",
            IsBanned = true,
        };

        await using (var seedCtx = CreateContext(tenantId, dbName))
        {
            seedCtx.Users.Add(bannedUser);
            seedCtx.Set<RefreshToken>().Add(new RefreshToken
            {
                TokenHash = "hash-banned-user",
                UserId = bannedUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
            });
            await seedCtx.SaveChangesAsync();
        }

        await using var anonymousCtx = CreateContext(null, dbName);
        var repository = new RefreshTokenRepository(anonymousCtx);

        var storedToken = await repository.GetByTokenHashAsync("hash-banned-user");

        Assert.NotNull(storedToken?.User);
        Assert.True(storedToken!.User.IsBanned);
    }
}

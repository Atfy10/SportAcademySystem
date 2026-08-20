using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Implementations;

namespace SportAcademy.Tests.Infrastructure.Implementations;

// Regression coverage for fixes B1 (cascade-revoke a token family on reuse detection) and B2
// (atomic, race-safe rotation) in JwtTokenService.ValidateAndRefreshTokenAsync.
//
// RefreshTokenRepository.TryRevokeAsync (the real EF implementation) relies on
// ExecuteUpdateAsync for a single atomic conditional UPDATE - real SQL Server supports this
// fine, but the EF Core InMemory test provider does not, and the full ApplicationDbContext
// model also isn't Sqlite-compatible (it uses a SQL Server sequence elsewhere). These tests
// exercise JwtTokenService's own logic against a small hand-rolled IRefreshTokenRepository
// fake instead, which lets them assert the exact contract JwtTokenService depends on
// (TryRevokeAsync's conditional semantics, RevokeAllUserTokensAsync being invoked) without
// needing a working relational provider in the test host.
public class JwtTokenServiceTests
{
    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly Dictionary<int, RefreshToken> _byId = new();
        private int _nextId = 1;

        public Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken ct = default)
        {
            token.Id = _nextId++;
            _byId[token.Id] = token;
            return Task.FromResult(token);
        }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
            Task.FromResult(_byId.Values.FirstOrDefault(t => t.TokenHash == tokenHash));

        public Task<RefreshToken?> GetByIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(_byId.TryGetValue(id, out var t) ? t : null);

        public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
        {
            _byId[token.Id] = token;
            return Task.CompletedTask;
        }

        public Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(_byId.Values.Where(t => t.UserId == userId && !t.IsRevoked).ToList());

        public Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
        {
            foreach (var t in _byId.Values.Where(t => t.UserId == userId && !t.IsRevoked))
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        public Task<bool> TryRevokeAsync(int tokenId, DateTime revokedAt, CancellationToken ct = default)
        {
            if (!_byId.TryGetValue(tokenId, out var t) || t.IsRevoked)
                return Task.FromResult(false);

            t.IsRevoked = true;
            t.RevokedAt = revokedAt;
            return Task.FromResult(true);
        }
    }

    private static IConfiguration CreateConfiguration()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("test-only-signing-key-not-used-outside-unit-tests-1234567890-abcdefghijklmnop");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("SportAcademy.Tests");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("SportAcademy.Tests");
        configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        return configMock.Object;
    }

    private static Mock<RoleManager<AppRole>> CreateRoleManagerMock() =>
        new(Mock.Of<IRoleStore<AppRole>>(), null!, null!, null!, null!);

    private static AppUser CreateUser(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UserName = "refresh-flow-user",
        Email = "refresh-flow-user@test.com",
        UserRoles = [],
    };

    [Fact]
    public async Task ValidateAndRefreshTokenAsync_BannedUser_RejectsRefresh()
    {
        // RefreshTokenRepository.GetByTokenHashAsync bypasses query filters (needed for the
        // anonymous-request tenant-null case - see RefreshTokenRepositoryTests), so it can
        // return a banned/deleted user's token too. JwtTokenService must reject it explicitly.
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        user.IsBanned = true;
        var repository = new FakeRefreshTokenRepository();
        var service = new JwtTokenService(CreateConfiguration(), repository, CreateRoleManagerMock().Object);

        const string plainToken = "plain-refresh-token-banned";
        await repository.AddAsync(new RefreshToken
        {
            TokenHash = service.HashToken(plainToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            User = user,
        });

        var result = await service.ValidateAndRefreshTokenAsync(plainToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReplayingRotatedToken_CascadeRevokesDescendantToken()
    {
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        var repository = new FakeRefreshTokenRepository();
        var service = new JwtTokenService(CreateConfiguration(), repository, CreateRoleManagerMock().Object);

        const string plainTokenA = "plain-refresh-token-A";
        await repository.AddAsync(new RefreshToken
        {
            TokenHash = service.HashToken(plainTokenA),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            User = user,
        });

        // Legitimate rotation: A -> B.
        var firstRefresh = await service.ValidateAndRefreshTokenAsync(plainTokenA);
        Assert.NotNull(firstRefresh);
        var plainTokenB = firstRefresh!.RefreshToken;

        // Attacker replays the now-revoked token A.
        var replayResult = await service.ValidateAndRefreshTokenAsync(plainTokenA);
        Assert.Null(replayResult);

        var hashB = service.HashToken(plainTokenB);
        var tokenB = await repository.GetByTokenHashAsync(hashB);

        // FIXED: replaying the compromised parent A now cascade-revokes the still-live
        // descendant token B, closing out the whole session chain.
        Assert.NotNull(tokenB);
        Assert.True(tokenB!.IsRevoked);

        // B is no longer usable either - the compromised session chain is fully dead.
        var refreshUsingB = await service.ValidateAndRefreshTokenAsync(plainTokenB);
        Assert.Null(refreshUsingB);
    }

    [Fact]
    public async Task ReplayingRotatedToken_InvokesRevokeAllUserTokens()
    {
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);

        var storedToken = new RefreshToken
        {
            Id = 1,
            TokenHash = "irrelevant-in-this-mock-based-test",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            User = user,
        };

        // The repository is mocked wholesale, so the exact hash value doesn't matter here (hash
        // correctness is covered by ReplayingRotatedToken_CascadeRevokesDescendantToken, which
        // exercises real hashing). This mock always returns the same mutable `storedToken`
        // instance regardless of which hash is looked up.
        var repositoryMock = new Mock<IRefreshTokenRepository>();
        repositoryMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        repositoryMock
            .Setup(r => r.TryRevokeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, DateTime _, CancellationToken _) => !storedToken.IsRevoked);
        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken t, CancellationToken _) => t);
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new JwtTokenService(CreateConfiguration(), repositoryMock.Object, CreateRoleManagerMock().Object);

        var firstRefresh = await service.ValidateAndRefreshTokenAsync("plain-A");
        Assert.NotNull(firstRefresh);

        // The mock's GetByTokenHashAsync always returns the same mutable `storedToken` instance,
        // which the first call already flipped to IsRevoked=true, so this second call replays
        // the exact real-world "revoked token presented again" path.
        var replayResult = await service.ValidateAndRefreshTokenAsync("plain-A");
        Assert.Null(replayResult);
        Assert.True(storedToken.IsRevoked);

        // FIXED: RevokeAllUserTokensAsync - which cascade-revokes a user's whole session chain
        // - is now invoked exactly once, in response to the replay of a revoked token.
        repositoryMock.Verify(
            r => r.RevokeAllUserTokensAsync(storedToken.UserId, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task TryRevokeAsync_SecondCallForAnAlreadyRevokedToken_ReturnsFalse()
    {
        // Proves the fake's TryRevokeAsync mirrors the real repository's conditional-UPDATE
        // contract: the second of two attempts to revoke the same token loses.
        var repository = new FakeRefreshTokenRepository();
        var user = CreateUser(Guid.NewGuid());
        var token = new RefreshToken
        {
            TokenHash = "hash-under-test",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            User = user,
        };
        await repository.AddAsync(token);

        var firstCallWon = await repository.TryRevokeAsync(token.Id, DateTime.UtcNow);
        var secondCallWon = await repository.TryRevokeAsync(token.Id, DateTime.UtcNow);

        Assert.True(firstCallWon);
        Assert.False(secondCallWon);
    }

    [Fact]
    public async Task ValidateAndRefreshTokenAsync_WhenRotationLosesRace_ReturnsNullWithoutMintingNewToken()
    {
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);

        var storedToken = new RefreshToken
        {
            Id = 1,
            TokenHash = "irrelevant-in-this-mock-based-test",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            User = user,
        };

        var repositoryMock = new Mock<IRefreshTokenRepository>();
        repositoryMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        // Simulates having lost a concurrent rotation race: another request already flipped
        // IsRevoked between the read above and here.
        repositoryMock
            .Setup(r => r.TryRevokeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new JwtTokenService(CreateConfiguration(), repositoryMock.Object, CreateRoleManagerMock().Object);

        var result = await service.ValidateAndRefreshTokenAsync("plain-A");

        Assert.Null(result);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

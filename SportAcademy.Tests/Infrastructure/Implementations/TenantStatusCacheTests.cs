using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Implementations;

namespace SportAcademy.Tests.Infrastructure.Implementations;

public class TenantStatusCacheTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly TenantStatusCache _cache;

    public TenantStatusCacheTests()
    {
        _cache = new TenantStatusCache(_tenantRepoMock.Object, _memoryCache);
    }

    private static Tenant CreateTenant(Guid id, TenantStatus status) => new()
    {
        Id = id,
        Name = "Test Academy",
        Slug = "test-academy",
        Status = status,
    };

    [Fact]
    public async Task GetStatusAsync_FirstCall_ReadsFromRepository()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId, TenantStatus.Active));

        var status = await _cache.GetStatusAsync(tenantId);

        status.Should().Be(TenantStatus.Active);
        _tenantRepoMock.Verify(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_SecondCall_ServesFromCacheWithoutHittingRepository()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId, TenantStatus.Suspended));

        await _cache.GetStatusAsync(tenantId);
        var second = await _cache.GetStatusAsync(tenantId);

        second.Should().Be(TenantStatus.Suspended);
        _tenantRepoMock.Verify(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_TenantNotFound_ReturnsNullAndCachesIt()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var first = await _cache.GetStatusAsync(tenantId);
        var second = await _cache.GetStatusAsync(tenantId);

        first.Should().BeNull();
        second.Should().BeNull();
        _tenantRepoMock.Verify(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Invalidate_ForcesNextReadToHitRepositoryAgain()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock
            .SetupSequence(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId, TenantStatus.Active))
            .ReturnsAsync(CreateTenant(tenantId, TenantStatus.Suspended));

        var before = await _cache.GetStatusAsync(tenantId);
        _cache.Invalidate(tenantId);
        var after = await _cache.GetStatusAsync(tenantId);

        before.Should().Be(TenantStatus.Active);
        after.Should().Be(TenantStatus.Suspended);
        _tenantRepoMock.Verify(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetStatusAsync_DifferentTenants_AreCachedIndependently()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantA, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantA, TenantStatus.Active));
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantB, TenantStatus.Archived));

        var statusA = await _cache.GetStatusAsync(tenantA);
        var statusB = await _cache.GetStatusAsync(tenantB);

        statusA.Should().Be(TenantStatus.Active);
        statusB.Should().Be(TenantStatus.Archived);
    }
}

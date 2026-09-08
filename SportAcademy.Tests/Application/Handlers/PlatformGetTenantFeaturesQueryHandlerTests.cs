using FluentAssertions;
using Moq;
using SportAcademy.Application.Queries.PlatformQueries.GetTenantFeatures;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

// The SuperAdmin's per-tenant Features tab must show the same category grouping and lock state
// as the Owner's own Settings > Features page for the same tenant - this locks in that the
// Platform-side query actually carries that information now (it didn't before: see
// TenantFeatureResponse's own comment).
public class PlatformGetTenantFeaturesQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly GetTenantFeaturesQueryHandler _handler;

    public PlatformGetTenantFeaturesQueryHandlerTests()
    {
        _handler = new GetTenantFeaturesQueryHandler(_tenantRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCategoryAndLockedBySuperAdminForEachFeature()
    {
        var tenantId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Test Academy", Slug = "test-academy" };
        var feature = new Feature { Id = featureId, Name = "attendance-tracking", DisplayName = "Attendance Tracking" };
        var tenantFeature = new TenantFeature
        {
            TenantId = tenantId,
            FeatureId = featureId,
            IsEnabled = false,
            EnabledBy = "TenantAdmin",
            LockedBySuperAdmin = false,
        };

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([feature]);
        _tenantRepoMock
            .Setup(r => r.GetTenantFeaturesAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([tenantFeature]);

        var result = await _handler.Handle(new GetTenantFeaturesQuery(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data!.Single();
        dto.Category.Should().Be("Operations");
        dto.IsEnabled.Should().BeFalse();
        dto.LockedBySuperAdmin.Should().BeFalse();
        dto.EnabledBy.Should().Be("TenantAdmin");
        dto.DependsOn.Should().Contain(["session-management", "enrollment-management"]);
        dto.RequiredBy.Should().BeEmpty();
    }
}

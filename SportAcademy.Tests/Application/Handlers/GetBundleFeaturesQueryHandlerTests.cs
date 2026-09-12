using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.PublicQueries.GetBundleFeatures;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Tests.Application.Handlers;

public class GetBundleFeaturesQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly GetBundleFeaturesQueryHandler _handler;

    public GetBundleFeaturesQueryHandlerTests()
    {
        _handler = new GetBundleFeaturesQueryHandler(_tenantRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ExcludesNotImplementedFeatures()
    {
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new Feature { Id = Guid.NewGuid(), Name = "trainee-management", DisplayName = "Trainees", IsImplemented = true, BundlePrice = 35, IsBundleCore = true },
            new Feature { Id = Guid.NewGuid(), Name = "chat-system", DisplayName = "Chat", IsImplemented = false, BundlePrice = 0, IsBundleCore = false },
        ]);

        var result = await _handler.Handle(new GetBundleFeaturesQuery(), CancellationToken.None);

        result.Data.Should().ContainSingle(f => f.Name == "trainee-management");
    }

    [Fact]
    public async Task Handle_DropsPrerequisiteReferencesToNotImplementedFeatures()
    {
        // session-management's real FeatureDependencies prerequisite includes
        // schedule-management, which is IsImplemented:false in the current catalog - the
        // response must not reference a feature it isn't also returning.
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new Feature { Id = Guid.NewGuid(), Name = "session-management", DisplayName = "Sessions", IsImplemented = true },
            new Feature { Id = Guid.NewGuid(), Name = "group-management", DisplayName = "Groups", IsImplemented = true },
            new Feature { Id = Guid.NewGuid(), Name = "schedule-management", DisplayName = "Schedule", IsImplemented = false },
        ]);

        var result = await _handler.Handle(new GetBundleFeaturesQuery(), CancellationToken.None);

        var session = result.Data!.Single(f => f.Name == "session-management");
        session.DirectPrerequisites.Should().Contain("group-management");
        session.DirectPrerequisites.Should().NotContain("schedule-management");
    }

    [Fact]
    public async Task Handle_ReflectsIsCoreAndPriceFromTheEntity()
    {
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new Feature { Id = Guid.NewGuid(), Name = "financial-reports", DisplayName = "Financial Reports", IsImplemented = true, BundlePrice = 10, IsBundleCore = false },
        ]);

        var result = await _handler.Handle(new GetBundleFeaturesQuery(), CancellationToken.None);

        var f = result.Data!.Single();
        f.BundlePrice.Should().Be(10);
        f.IsCore.Should().BeFalse();
        f.Category.Should().Be("Finance");
    }
}

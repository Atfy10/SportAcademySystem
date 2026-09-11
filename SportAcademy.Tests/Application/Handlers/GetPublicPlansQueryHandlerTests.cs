using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.PublicQueries.GetPublicPlans;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

public class GetPublicPlansQueryHandlerTests
{
    private readonly Mock<IBaseRepository<SubscriptionPlan, int>> _planRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly GetPublicPlansQueryHandler _handler;

    public GetPublicPlansQueryHandlerTests()
    {
        _handler = new GetPublicPlansQueryHandler(_planRepoMock.Object, _tenantRepoMock.Object);
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task Handle_HidesInactiveAndUnlistedPlans()
    {
        _planRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new SubscriptionPlan { Id = 1, Name = "Basic", Code = "BASIC", IsActive = true, IsPubliclyListed = true, DisplayOrder = 1 },
            new SubscriptionPlan { Id = 2, Name = "Legacy", Code = "LEGACY", IsActive = false, IsPubliclyListed = true, DisplayOrder = 2 },
            new SubscriptionPlan { Id = 3, Name = "Internal", Code = "INTERNAL", IsActive = true, IsPubliclyListed = false, DisplayOrder = 3 },
        ]);

        var result = await _handler.Handle(new GetPublicPlansQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle(p => p.Name == "Basic");
    }

    [Fact]
    public async Task Handle_OrdersByDisplayOrderThenPrice()
    {
        _planRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new SubscriptionPlan { Id = 1, Name = "Enterprise", Code = "ENTERPRISE", IsActive = true, IsPubliclyListed = true, DisplayOrder = 3, MonthlyPrice = 199 },
            new SubscriptionPlan { Id = 2, Name = "Basic", Code = "BASIC", IsActive = true, IsPubliclyListed = true, DisplayOrder = 1, MonthlyPrice = 49 },
            new SubscriptionPlan { Id = 3, Name = "Professional", Code = "PRO", IsActive = true, IsPubliclyListed = true, DisplayOrder = 2, MonthlyPrice = 99 },
        ]);

        var result = await _handler.Handle(new GetPublicPlansQuery(), CancellationToken.None);

        result.Data!.Select(p => p.Name).Should().Equal("Basic", "Professional", "Enterprise");
    }

    [Fact]
    public async Task Handle_IncludesGrantedFeatureDisplayNames()
    {
        var featureId = Guid.NewGuid();
        _planRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new SubscriptionPlan { Id = 1, Name = "Basic", Code = "BASIC", IsActive = true, IsPubliclyListed = true },
        ]);
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Feature { Id = featureId, Name = "trainee-management", DisplayName = "Trainees" }]);
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([featureId]);

        var result = await _handler.Handle(new GetPublicPlansQuery(), CancellationToken.None);

        result.Data!.Single().Features.Should().Equal("Trainees");
    }
}

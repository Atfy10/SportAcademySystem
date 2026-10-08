using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TenantQueries.GetMyLimitUsage;

namespace SportAcademy.Tests.Application.Handlers;

public class GetMyLimitUsageQueryHandlerTests
{
    private readonly Mock<IUserContextService> _userContext = new();
    private readonly Mock<IEffectiveLimitService> _limits = new();

    [Fact]
    public async Task Handle_ReturnsCallerTenantsLimits_WithoutTheSuperAdminsOverrideReason()
    {
        var tenantId = Guid.NewGuid();
        _userContext.Setup(c => c.TenantId).Returns(tenantId);
        _limits.Setup(l => l.GetAllAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new EffectiveLimit("trainees", 50, LimitSource.Override, 48, "Pilot discount - internal"),
            new EffectiveLimit("branches", null, LimitSource.Unlimited, 3, null),
        ]);

        var handler = new GetMyLimitUsageQueryHandler(_userContext.Object, _limits.Object);
        var result = await handler.Handle(new GetMyLimitUsageQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        var trainees = result.Data!.Single(l => l.ResourceKey == "trainees");
        trainees.MaxCount.Should().Be(50);
        trainees.Used.Should().Be(48);
        trainees.OverrideReason.Should().BeNull();
        result.Data!.Single(l => l.ResourceKey == "branches").MaxCount.Should().BeNull();
    }
}

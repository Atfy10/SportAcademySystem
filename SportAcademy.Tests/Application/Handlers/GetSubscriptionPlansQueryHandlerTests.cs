using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.PlatformQueries.GetSubscriptionPlans;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

public class GetSubscriptionPlansQueryHandlerTests
{
    private readonly Mock<IBaseRepository<SubscriptionPlan, int>> _planRepoMock = new();
    private readonly GetSubscriptionPlansQueryHandler _handler;

    public GetSubscriptionPlansQueryHandlerTests()
    {
        _handler = new GetSubscriptionPlansQueryHandler(_planRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyActivePlansOrderedByPrice()
    {
        _planRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new SubscriptionPlan { Id = 3, Name = "Enterprise", Code = "ENTERPRISE", MonthlyPrice = 300, IsActive = true },
            new SubscriptionPlan { Id = 1, Name = "Basic", Code = "BASIC", MonthlyPrice = 100, IsActive = true },
            new SubscriptionPlan { Id = 4, Name = "Retired", Code = "OLD", MonthlyPrice = 50, IsActive = false },
        ]);

        var result = await _handler.Handle(new GetSubscriptionPlansQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.Select(p => p.Id).Should().Equal(1, 3);
    }
}

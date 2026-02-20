using FluentAssertions;
using MediatR;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Tests.Application.Behaviors;

public class PaginationNormalizationBehaviorTests
{
    private readonly PaginationNormalizationBehavior<TestPaginatedRequest, Result<string>> _behavior = new();

    [Fact]
    public async Task Handle_NormalizesPageRequest()
    {
        // Page with invalid values — should be normalized
        var request = new TestPaginatedRequest
        {
            Page = PageRequest.Create(-1, 500)
        };

        var result = await _behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        // After normalization: page=-1 → 1, size=500 → MaxPageSize(100)
        request.Page.Page.Should().Be(1);
        request.Page.PageSize.Should().Be(PageRequest.MaxPageSize);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidPageRequest_PassesThrough()
    {
        var request = new TestPaginatedRequest
        {
            Page = PageRequest.Create(3, 25)
        };

        await _behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        request.Page.Page.Should().Be(3);
        request.Page.PageSize.Should().Be(25);
    }

    private class TestPaginatedRequest : IPaginatedRequest, IRequest<Result<string>>
    {
        public PageRequest Page { get; set; } = PageRequest.Create(1, 10);
    }
}

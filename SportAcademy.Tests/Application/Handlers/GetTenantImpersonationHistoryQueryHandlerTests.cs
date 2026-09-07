using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TenantQueries.GetTenantImpersonationHistory;

namespace SportAcademy.Tests.Application.Handlers;

public class GetTenantImpersonationHistoryQueryHandlerTests
{
    private readonly Mock<IImpersonationGrantRepository> _grantRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly GetTenantImpersonationHistoryQueryHandler _handler;

    public GetTenantImpersonationHistoryQueryHandlerTests()
    {
        _handler = new GetTenantImpersonationHistoryQueryHandler(_grantRepoMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ReturnsFailure400()
    {
        _userContextMock.Setup(c => c.TenantId).Returns((Guid?)null);

        var result = await _handler.Handle(new GetTenantImpersonationHistoryQuery(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ValidTenantContext_ReturnsThatTenantsGrantsOnly()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);

        var expected = new PagedData<TenantImpersonationGrantDto>
        {
            Items = [new TenantImpersonationGrantDto(Guid.NewGuid(), tenantId, "Jane SuperAdmin", "reason", DateTime.UtcNow, DateTime.UtcNow, null, null, true)],
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
        };
        _grantRepoMock
            .Setup(r => r.GetPagedForTenantAsync(tenantId, It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new GetTenantImpersonationHistoryQuery(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(expected);
        _grantRepoMock.Verify(r => r.GetPagedForTenantAsync(tenantId, It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

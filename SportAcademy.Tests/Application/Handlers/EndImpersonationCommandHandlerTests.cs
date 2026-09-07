using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.EndImpersonation;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

public class EndImpersonationCommandHandlerTests
{
    private readonly Mock<IImpersonationGrantRepository> _grantRepoMock = new();
    private readonly EndImpersonationCommandHandler _handler;

    public EndImpersonationCommandHandlerTests()
    {
        _handler = new EndImpersonationCommandHandler(_grantRepoMock.Object);
    }

    private static TenantImpersonationGrant CreateGrant(Guid id, Guid tenantId, DateTime? endedAt = null) => new()
    {
        Id = id,
        TenantId = tenantId,
        GrantedByUserId = Guid.NewGuid(),
        Reason = "reason",
        StartedAt = DateTime.UtcNow.AddMinutes(-5),
        ExpiresAt = DateTime.UtcNow.AddMinutes(55),
        EndedAt = endedAt,
    };

    [Fact]
    public async Task Handle_GrantNotFound_ReturnsFailure404()
    {
        var grantId = Guid.NewGuid();
        _grantRepoMock.Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>())).ReturnsAsync((TenantImpersonationGrant?)null);

        var result = await _handler.Handle(new EndImpersonationCommand(grantId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_AlreadyEnded_ReturnsFailureButStillResolvesTenantId()
    {
        var grantId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var grant = CreateGrant(grantId, tenantId, endedAt: DateTime.UtcNow.AddMinutes(-1));
        _grantRepoMock.Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>())).ReturnsAsync(grant);

        var command = new EndImpersonationCommand(grantId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        // Even a failed EndImpersonation should still be attributed to the right tenant in the
        // audit event PlatformAuditBehavior writes.
        ((IAuditableCommand)command).AuditTenantId.Should().Be(tenantId);
        _grantRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TenantImpersonationGrant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveGrant_EndsItWithManualReason()
    {
        var grantId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var grant = CreateGrant(grantId, tenantId);
        _grantRepoMock.Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>())).ReturnsAsync(grant);

        var result = await _handler.Handle(new EndImpersonationCommand(grantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        grant.EndedAt.Should().NotBeNull();
        grant.EndedReason.Should().Be("Manual");
        _grantRepoMock.Verify(r => r.UpdateAsync(grant, It.IsAny<CancellationToken>()), Times.Once);
    }
}

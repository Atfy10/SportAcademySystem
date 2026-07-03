using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.TenantCommands.ActivateTenant;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class ActivateTenantCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Tenant, Guid>> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly ActivateTenantCommandHandler _handler;

    public ActivateTenantCommandHandlerTests()
    {
        _handler = new ActivateTenantCommandHandler(
            _tenantRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    private static Tenant CreateTenant(Guid id, TenantStatus status = TenantStatus.PendingSetup) => new()
    {
        Id = id,
        Name = "Test Academy",
        Slug = "test-academy",
        Status = status
    };

    [Fact]
    public async Task Handle_PendingSetupTenant_TransitionsToActive()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.PendingSetup);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ActivateTenantCommand(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyActiveTenant_ReturnsSuccessNoChange()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ActivateTenantCommand(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var result = await _handler.Handle(
            new ActivateTenantCommand(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_SuspendedTenant_DoesNotTransition()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Suspended);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ActivateTenantCommand(tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Suspended);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.CreateInvitation;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class CreateInvitationCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Tenant, Guid>> _tenantRepoMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly CreateInvitationCommandHandler _handler;

    public CreateInvitationCommandHandlerTests()
    {
        _handler = new CreateInvitationCommandHandler(
            _tenantRepoMock.Object,
            _tokenServiceMock.Object,
            _invitationRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object,
            _tenantIdProviderMock.Object);
    }

    private static Tenant CreateTenant(Guid id, string slug = "test-academy") => new()
    {
        Id = id,
        Name = "Test Academy",
        Slug = slug,
        Status = TenantStatus.PendingSetup
    };

    private static CreateInvitationCommand CreateValidCommand(Guid tenantId) =>
        new(tenantId, "owner@test.com", Guid.NewGuid());

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var tenantId = Guid.NewGuid();
        var command = CreateValidCommand(tenantId);
        var tenant = CreateTenant(tenantId);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _tokenServiceMock
            .Setup(s => s.GenerateRawToken())
            .Returns("raw-token-value");

        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token-value"))
            .Returns("hashed-token-value");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("owner@test.com");

        _invitationRepoMock.Verify(
            r => r.AddAsync(It.Is<Invitation>(i =>
                i.Email == "owner@test.com" &&
                i.TenantId == tenantId &&
                i.Status == InvitationStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var command = CreateValidCommand(tenantId);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}

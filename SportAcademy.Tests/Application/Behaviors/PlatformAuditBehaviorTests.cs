using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Behaviors;

public class PlatformAuditBehaviorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantAuditRepository> _auditRepositoryMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly PlatformAuditBehavior<AuditableRequest, Result<string>> _behavior;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    public PlatformAuditBehaviorTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_userId);
        _userContextMock.Setup(c => c.IpAddress).Returns("203.0.113.5");
        _userContextMock.Setup(c => c.UserAgent).Returns("test-agent");
        _userRepositoryMock
            .Setup(r => r.GetDisplayNameAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Jane SuperAdmin");

        _behavior = new PlatformAuditBehavior<AuditableRequest, Result<string>>(
            _unitOfWorkMock.Object,
            _auditRepositoryMock.Object,
            _userContextMock.Object,
            _userRepositoryMock.Object,
            Mock.Of<ILogger<PlatformAuditBehavior<AuditableRequest, Result<string>>>>());
    }

    [Fact]
    public async Task Handle_NonAuditableRequest_CallsNextWithoutTouchingUnitOfWork()
    {
        var behavior = new PlatformAuditBehavior<PlainRequest, Result<string>>(
            _unitOfWorkMock.Object,
            _auditRepositoryMock.Object,
            _userContextMock.Object,
            _userRepositoryMock.Object,
            Mock.Of<ILogger<PlatformAuditBehavior<PlainRequest, Result<string>>>>());

        var result = await behavior.Handle(
            new PlainRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepositoryMock.Verify(
            r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SuccessfulCommand_WritesSucceededEventInsideTransaction()
    {
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        var result = await _behavior.Handle(
            new AuditableRequest(_tenantId),
            _ => Task.FromResult(Result<string>.Success("ok", "Test", "Tenant archived successfully.")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(_tenantId);
        captured.EventType.Should().Be("test.audited");
        captured.Description.Should().Be("Tenant archived successfully.");
        captured.Outcome.Should().Be(AuditOutcome.Succeeded);
        captured.PerformedByUserId.Should().Be(_userId);
        captured.PerformedBy.Should().Be("Jane SuperAdmin");
        captured.IpAddress.Should().Be("203.0.113.5");
        captured.UserAgent.Should().Be("test-agent");
        captured.AfterJson.Should().Contain(_tenantId.ToString());
    }

    [Fact]
    public async Task Handle_FailedCommand_StillCommitsAFailedEvent()
    {
        // A denied/rejected business rule is exactly the case the old system never recorded at
        // all (F-08) - the audit write must not be skipped just because the command failed.
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        var result = await _behavior.Handle(
            new AuditableRequest(_tenantId),
            _ => Task.FromResult(Result<string>.Failure("Test", "Cannot archive an Active tenant.", 400)),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        captured.Should().NotBeNull();
        captured!.Outcome.Should().Be(AuditOutcome.Failed);
        captured.Description.Should().Be("Cannot archive an Active tenant.");
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_HandlerThrows_RollsBackAndWritesNoAuditEvent()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(
                new AuditableRequest(_tenantId),
                _ => throw new InvalidOperationException("boom"),
                CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditRepositoryMock.Verify(
            r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CommandSetsResolvedBeforeState_SerializesItToBeforeJson()
    {
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        var request = new AuditableRequest(_tenantId);

        await _behavior.Handle(
            request,
            _ =>
            {
                // Simulates a handler capturing the entity's state before mutating it, the same
                // way ArchiveTenantCommandHandler etc. set ResolvedBeforeState on `request`
                // before applying their own change.
                request.ResolvedBeforeState = new { Status = "Active" };
                return Task.FromResult(Result<string>.Success("ok", "Test"));
            },
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.BeforeJson.Should().Contain("Active");
    }

    [Fact]
    public async Task Handle_CommandLeavesResolvedBeforeStateNull_WritesNullBeforeJson()
    {
        // CreateTenantCommand/StartImpersonationCommand and similar "there is no prior entity"
        // commands never set ResolvedBeforeState - BeforeJson must stay null for them rather
        // than serializing to the literal string "null".
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        await _behavior.Handle(
            new AuditableRequest(_tenantId),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.BeforeJson.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NullAuditTenantId_WritesEventWithNoTenant()
    {
        // CreateTenantCommand-that-failed / a platform-wide denial: legitimately no single
        // tenant to attribute the event to (F-08 wanted these recorded too, not skipped).
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddWithoutSaveAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        await _behavior.Handle(
            new AuditableRequest(null),
            _ => Task.FromResult(Result<string>.Failure("Test", "Slug already in use.", 400)),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoAuthenticatedUser_ThrowsWithoutOpeningATransaction()
    {
        _userContextMock.Setup(c => c.UserId).Returns((Guid?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(
                new AuditableRequest(_tenantId),
                _ => Task.FromResult(Result<string>.Success("ok", "Test")),
                CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    public record AuditableRequest(Guid? AuditTenantId) : IRequest<Result<string>>, IAuditableCommand
    {
        public string AuditEventType => "test.audited";
        Guid? IAuditableCommand.AuditTenantId => AuditTenantId;

        public object? ResolvedBeforeState { get; set; }
        object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
    }

    public record PlainRequest : IRequest<Result<string>>;
}

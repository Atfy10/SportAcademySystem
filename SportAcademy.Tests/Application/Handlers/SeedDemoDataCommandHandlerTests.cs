using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.SeedDemoData;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Tests.Application.Handlers;

public class SeedDemoDataCommandHandlerTests
{
    private readonly Mock<IHostEnvironmentInfo> _environmentMock = new();
    private readonly Mock<IDemoDataSeeder> _seederMock = new();
    private readonly SeedDemoDataCommandHandler _handler;

    public SeedDemoDataCommandHandlerTests()
    {
        _handler = new SeedDemoDataCommandHandler(_environmentMock.Object, _seederMock.Object);
    }

    [Fact]
    public async Task Handle_NotDevelopment_Returns403WithCode_AndNeverTouchesTheSeeder()
    {
        _environmentMock.SetupGet(e => e.IsDevelopment).Returns(false);
        var command = new SeedDemoDataCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Code.Should().Be(DemoDataErrorCodes.NotDevelopment);
        command.ResolvedTenantId.Should().BeNull();
        _seederMock.Verify(s => s.SeedAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadySeeded_Returns409WithCode_AndAttributesTheAuditEventToTheExistingTenant()
    {
        var tenantId = Guid.NewGuid();
        _environmentMock.SetupGet(e => e.IsDevelopment).Returns(true);
        _seederMock
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DemoSeedResult(DemoSeedOutcome.AlreadySeeded, tenantId, "aura-demo", "owner"));
        var command = new SeedDemoDataCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Code.Should().Be(DemoDataErrorCodes.AlreadySeeded);
        ((IAuditableCommand)command).AuditTenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_Seeded_Returns200WithLoginDetails_AndNoErrorCode()
    {
        var tenantId = Guid.NewGuid();
        _environmentMock.SetupGet(e => e.IsDevelopment).Returns(true);
        _seederMock
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DemoSeedResult(DemoSeedOutcome.Seeded, tenantId, "aura-demo", "demo.owner"));
        var command = new SeedDemoDataCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Code.Should().BeNull();
        result.Data.Should().Be(new SeedDemoDataDto("aura-demo", "demo.owner"));
        ((IAuditableCommand)command).AuditTenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Command_IsAuditedAsDemoSeed_WithNoBeforeState()
    {
        IAuditableCommand command = new SeedDemoDataCommand();

        command.AuditEventType.Should().Be("demo.seed");
        command.AuditBeforeState.Should().BeNull();
    }
}

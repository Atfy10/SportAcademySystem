using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.TenantCommands.UpdateTenantProfile;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateTenantProfileCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly UpdateTenantProfileCommandHandler _handler;

    public UpdateTenantProfileCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.TenantId).Returns(TenantId);
        _handler = new UpdateTenantProfileCommandHandler(
            _tenantRepoMock.Object, _unitOfWorkMock.Object, _userContextMock.Object, _fileStorageMock.Object);
    }

    private static UpdateTenantProfileCommand EmptyUpdate(bool markSetupComplete = false) =>
        new(null, null, null, null, null, null, null, null, null, markSetupComplete);

    [Fact]
    public async Task Handle_MarkSetupCompleteTrue_SetsIsSetupCompleteOnTheProfile()
    {
        var profile = new TenantProfile { TenantId = TenantId, OrganizationName = "Acme Academy", IsSetupComplete = false };
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var result = await _handler.Handle(EmptyUpdate(markSetupComplete: true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.IsSetupComplete.Should().BeTrue();
        _tenantRepoMock.Verify(r => r.UpdateProfile(profile), Times.Once);
    }

    [Fact]
    public async Task Handle_MarkSetupCompleteFalse_LeavesIsSetupCompleteUnchanged()
    {
        var profile = new TenantProfile { TenantId = TenantId, OrganizationName = "Acme Academy", IsSetupComplete = true };
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var result = await _handler.Handle(EmptyUpdate(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // An ordinary later edit (e.g. from Settings) must never flip it back off.
        profile.IsSetupComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LogoUrlChanged_DeletesTheOldLogoFile()
    {
        var profile = new TenantProfile
        {
            TenantId = TenantId, OrganizationName = "Acme Academy", LogoUrl = "/uploads/tenant-logos/old.png",
        };
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var command = new UpdateTenantProfileCommand(
            null, "/uploads/tenant-logos/new.png", null, null, null, null, null, null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        profile.LogoUrl.Should().Be("/uploads/tenant-logos/new.png");
        _fileStorageMock.Verify(f => f.DeleteImage("/uploads/tenant-logos/old.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_LogoUrlUnchanged_DoesNotDeleteAnything()
    {
        var profile = new TenantProfile
        {
            TenantId = TenantId, OrganizationName = "Acme Academy", LogoUrl = "/uploads/tenant-logos/same.png",
        };
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var command = new UpdateTenantProfileCommand(
            null, "/uploads/tenant-logos/same.png", null, null, null, null, null, null, null);
        await _handler.Handle(command, CancellationToken.None);

        _fileStorageMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ReturnsFailure404()
    {
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantProfile?)null);

        var result = await _handler.Handle(EmptyUpdate(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}

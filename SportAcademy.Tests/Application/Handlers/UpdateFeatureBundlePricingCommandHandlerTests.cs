using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.UpdateFeatureBundlePricing;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdateFeatureBundlePricingCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Feature, Guid>> _featureRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateFeatureBundlePricingCommandHandler _handler;

    public UpdateFeatureBundlePricingCommandHandlerTests()
    {
        _handler = new UpdateFeatureBundlePricingCommandHandler(_featureRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_FeatureNotFound_ReturnsFailure404()
    {
        _featureRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature?)null);

        var result = await _handler.Handle(new UpdateFeatureBundlePricingCommand(Guid.NewGuid(), 10, false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_NotImplementedFeature_ReturnsFailure400()
    {
        var id = Guid.NewGuid();
        _featureRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Feature { Id = id, Name = "chat-system", DisplayName = "Chat", IsImplemented = false });

        var result = await _handler.Handle(new UpdateFeatureBundlePricingCommand(id, 10, false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _featureRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Success_UpdatesPriceAndCoreFlag()
    {
        var id = Guid.NewGuid();
        var feature = new Feature { Id = id, Name = "financial-reports", DisplayName = "Financial Reports", IsImplemented = true, BundlePrice = 5, IsBundleCore = false };
        _featureRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(feature);

        var result = await _handler.Handle(new UpdateFeatureBundlePricingCommand(id, 12.5m, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        feature.BundlePrice.Should().Be(12.5m);
        feature.IsBundleCore.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

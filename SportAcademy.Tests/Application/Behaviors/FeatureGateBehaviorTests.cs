using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Tests.Application.Behaviors;

public class FeatureGateBehaviorTests
{
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<ITenantRepository> _tenantRepositoryMock = new();
    private readonly FeatureGateBehavior<GatedRequest, Result<string>> _behavior;

    public FeatureGateBehaviorTests()
    {
        _behavior = new FeatureGateBehavior<GatedRequest, Result<string>>(
            _userContextMock.Object,
            _tenantRepositoryMock.Object,
            Mock.Of<ILogger<FeatureGateBehavior<GatedRequest, Result<string>>>>());
    }

    [Fact]
    public async Task Handle_RequestNotGated_CallsNextWithoutCheckingFeature()
    {
        var behavior = new FeatureGateBehavior<UngatedRequest, Result<string>>(
            _userContextMock.Object,
            _tenantRepositoryMock.Object,
            Mock.Of<ILogger<FeatureGateBehavior<UngatedRequest, Result<string>>>>());

        var result = await behavior.Handle(
            new UngatedRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tenantRepositoryMock.Verify(
            r => r.IsFeatureEnabledAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NoTenantContext_CallsNext()
    {
        _userContextMock.Setup(u => u.TenantId).Returns((Guid?)null);

        var result = await _behavior.Handle(
            new GatedRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FeatureEnabled_CallsNext()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.TenantId).Returns(tenantId);
        _tenantRepositoryMock
            .Setup(r => r.IsFeatureEnabledAsync(tenantId, "attendance-tracking", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _behavior.Handle(
            new GatedRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FeatureDisabled_ShortCircuitsWithForbiddenAndCode()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(u => u.TenantId).Returns(tenantId);
        _tenantRepositoryMock
            .Setup(r => r.IsFeatureEnabledAsync(tenantId, "attendance-tracking", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var nextCalled = false;
        var result = await _behavior.Handle(
            new GatedRequest(),
            _ => { nextCalled = true; return Task.FromResult(Result<string>.Success("ok", "Test")); },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Errors.Should().ContainKey("code");
        result.Errors!["code"].Should().Contain("FEATURE_DISABLED");
    }

    public record GatedRequest : IRequest<Result<string>>, IRequiresFeature
    {
        public string FeatureKey => "attendance-tracking";
    }

    public record UngatedRequest : IRequest<Result<string>>;
}

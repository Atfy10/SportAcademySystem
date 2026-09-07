using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Behaviors;

// Covers the TraceId-stamping fix: most business-rule rejections return Result.Failure(...)
// directly rather than throwing, and used to leave TraceId null - only the generic
// catch (Exception) branch ever stamped one. Every failure that passes through this behavior
// now gets a correlation reference, thrown or returned.
public class ExceptionHandlingBehaviorTests
{
    private readonly Mock<ILocalizationService> _localizerMock = new();
    private readonly ExceptionHandlingBehavior<PlainRequest, Result<string>> _behavior;

    public ExceptionHandlingBehaviorTests()
    {
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] _) => key);
        _localizerMock.Setup(l => l.Exists(It.IsAny<string>())).Returns(false);

        _behavior = new ExceptionHandlingBehavior<PlainRequest, Result<string>>(
            Mock.Of<ILogger<ExceptionHandlingBehavior<PlainRequest, Result<string>>>>(),
            _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_HandlerReturnsDirectFailure_StampsATraceId()
    {
        var result = await _behavior.Handle(
            new PlainRequest(),
            _ => Task.FromResult(Result<string>.Failure("Test", "Tenant is already archived.", 400)),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_HandlerReturnsSuccess_DoesNotStampATraceId()
    {
        var result = await _behavior.Handle(
            new PlainRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.TraceId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnhandledException_StampsATraceId()
    {
        var result = await _behavior.Handle(
            new PlainRequest(),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_IdNotFoundException_StampsATraceIdToo()
    {
        var result = await _behavior.Handle(
            new PlainRequest(),
            _ => throw new IdNotFoundException("Trainee", 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.TraceId.Should().NotBeNullOrEmpty();
    }

    public record PlainRequest : IRequest<Result<string>>;
}

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Tests.Application.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(
            Enumerable.Empty<IValidator<TestRequest>>());

        var result = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, Result<string>>(
            new[] { validatorMock.Object });

        var result = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required")
            }));

        var behavior = new ValidationBehavior<TestRequest, Result<string>>(
            new[] { validatorMock.Object });

        var act = () => behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(Result<string>.Success("ok", "Test")),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    public class TestRequest : IRequest<Result<string>> { }
}

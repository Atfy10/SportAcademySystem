using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdatePaymentStatusCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly UpdatePaymentStatusCommandHandler _handler;

    public UpdatePaymentStatusCommandHandlerTests()
    {
        _handler = new UpdatePaymentStatusCommandHandler(_enrollmentRepoMock.Object, _paymentRepoMock.Object);
    }

    private static UpdatePaymentStatusCommand CreateValidCommand(int enrollmentId = 1, string status = "Paid") =>
        new(Id: enrollmentId, PaymentStatus: status);

    private static Enrollment CreateEnrollment(int id = 1, int subId = 1) => new()
    {
        Id = id,
        TraineeId = 1,
        TraineeGroupId = 1,
        SubscriptionDetailsId = subId,
        EnrollmentDate = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(1),
        SessionAllowed = 8,
        SessionRemaining = 8,
        IsActive = true
    };

    [Fact]
    public async Task Handle_EnrollmentNotFound_ThrowsIdNotFoundException()
    {
        // Arrange
        var command = CreateValidCommand(999);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_PaidStatusWithNoExistingPayment_CreatesNewPayment()
    {
        // Arrange
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _paymentRepoMock.Setup(r => r.ExistsForSubscriptionAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PaidStatusWithExistingPayment_DoesNotCreateDuplicate()
    {
        // Arrange
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _paymentRepoMock.Setup(r => r.ExistsForSubscriptionAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonPaidStatus_SkipsPaymentLogic()
    {
        // Arrange
        var command = CreateValidCommand(1, "Pending");
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _paymentRepoMock.Verify(r => r.ExistsForSubscriptionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreatedPaymentHasCorrectFormat()
    {
        // Arrange
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);
        Payment? capturedPayment = null;

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _paymentRepoMock.Setup(r => r.ExistsForSubscriptionAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, ct) => capturedPayment = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.PaymentNumber.Should().StartWith("PAY-");
        capturedPayment.BranchId.Should().Be(1);
    }

    [Theory]
    [InlineData("Paid")]
    [InlineData("Pending")]
    [InlineData("Overdue")]
    public async Task Handle_VariousPaymentStatuses_ReturnsSuccess(string status)
    {
        // Arrange
        var command = CreateValidCommand(1, status);
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _paymentRepoMock.Setup(r => r.ExistsForSubscriptionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

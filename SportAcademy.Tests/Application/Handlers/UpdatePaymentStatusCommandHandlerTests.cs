using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class UpdatePaymentStatusCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock = new();
    private readonly Mock<IFinanceLedgerService> _financeLedgerServiceMock = new();
    private readonly Mock<IPaymentTypeRepository> _paymentTypeRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly UpdatePaymentStatusCommandHandler _handler;

    public UpdatePaymentStatusCommandHandlerTests()
    {
        _paymentTypeRepoMock.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentType { Id = 1, Name = "Cash", IsDefault = true });

        _handler = new UpdatePaymentStatusCommandHandler(
            _enrollmentRepoMock.Object, _invoiceRepoMock.Object, _financeLedgerServiceMock.Object,
            _paymentTypeRepoMock.Object, _userContextMock.Object);
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

    private static Invoice CreateInvoice(int id = 5, int branchId = 3, decimal grandTotal = 50m, decimal amountPaid = 0m) => new()
    {
        Id = id,
        InvoiceNumber = $"INV-2026-{id:D5}",
        Status = amountPaid >= grandTotal ? InvoiceStatus.Paid : InvoiceStatus.Issued,
        IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
        DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
        BranchId = branchId,
        Currency = "KWD",
        GrandTotal = grandTotal,
        AmountPaid = amountPaid,
    };

    [Fact]
    public async Task Handle_EnrollmentNotFound_ThrowsIdNotFoundException()
    {
        var command = CreateValidCommand(999);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_PaidStatusWithOutstandingBalance_RecordsPaymentForFullOutstanding()
    {
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);
        var invoice = CreateInvoice(id: 5, branchId: 3, grandTotal: 50m, amountPaid: 0m);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _invoiceRepoMock.Setup(r => r.GetBySubscriptionDetailsIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _financeLedgerServiceMock.Verify(s => s.RecordPaymentAsync(
            It.Is<RecordPaymentInput>(i => i.Amount == 50m && i.BranchId == 3
                && i.Allocations.Count == 1 && i.Allocations[0].InvoiceId == 5 && i.Allocations[0].Amount == 50m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PaidStatusWithNoOutstandingBalance_DoesNotRecordPayment()
    {
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);
        var invoice = CreateInvoice(id: 5, grandTotal: 50m, amountPaid: 50m);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _invoiceRepoMock.Setup(r => r.GetBySubscriptionDetailsIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _financeLedgerServiceMock.Verify(s => s.RecordPaymentAsync(
            It.IsAny<RecordPaymentInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaidStatusWithNoInvoice_ThrowsIdNotFoundException()
    {
        var command = CreateValidCommand(1, "Paid");
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _invoiceRepoMock.Setup(r => r.GetBySubscriptionDetailsIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_NonPaidStatus_SkipsPaymentLogic()
    {
        var command = CreateValidCommand(1, "Pending");
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _invoiceRepoMock.Verify(r => r.GetBySubscriptionDetailsIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _financeLedgerServiceMock.Verify(s => s.RecordPaymentAsync(
            It.IsAny<RecordPaymentInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Overdue")]
    public async Task Handle_NonPaidStatuses_ReturnSuccess(string status)
    {
        var command = CreateValidCommand(1, status);
        var enrollment = CreateEnrollment(1, 5);

        _enrollmentRepoMock.Setup(r => ((IBaseRepository<Enrollment, int>)r)
            .GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

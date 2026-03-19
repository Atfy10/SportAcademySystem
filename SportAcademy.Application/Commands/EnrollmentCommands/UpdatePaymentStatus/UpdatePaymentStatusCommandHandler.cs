using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;

public class UpdatePaymentStatusCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IPaymentRepository paymentRepository)
    : IRequestHandler<UpdatePaymentStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdatePaymentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Domain.Exceptions.BaseExceptions.IdNotFoundException("Enrollment", request.Id.ToString());

        if (request.PaymentStatus == "Paid")
        {
            var hasPayment = await paymentRepository.ExistsForSubscriptionAsync(
                enrollment.SubscriptionDetailsId, cancellationToken);
            if (!hasPayment)
            {
                var payment = new Payment
                {
                    PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    PaidDate = DateTime.UtcNow,
                    BranchId = 1
                };
                await paymentRepository.AddAsync(payment, cancellationToken);
            }
        }

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

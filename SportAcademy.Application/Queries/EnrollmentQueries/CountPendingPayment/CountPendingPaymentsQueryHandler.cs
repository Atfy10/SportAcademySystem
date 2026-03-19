using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountPendingPayment;

public class CountPendingPaymentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<CountPendingPaymentsQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        CountPendingPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var count = await enrollmentRepository.CountPendingPaymentAsync(cancellationToken);
        return Result<int>.Success(count, OperationType.Get.ToString());
    }
}

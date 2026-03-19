using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountAll;

public class CountAllEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<CountAllEnrollmentsQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        CountAllEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var count = await enrollmentRepository.CountAllAsync(cancellationToken);
        return Result<int>.Success(count, OperationType.Get.ToString());
    }
}

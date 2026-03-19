using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountActive;

public class CountActiveEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<CountActiveEnrollmentsQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        CountActiveEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var count = await enrollmentRepository.CountActiveAsync(cancellationToken);
        return Result<int>.Success(count, OperationType.Get.ToString());
    }
}

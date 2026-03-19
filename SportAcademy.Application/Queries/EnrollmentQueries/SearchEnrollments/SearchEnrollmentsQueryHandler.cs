using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.SearchEnrollments;

public class SearchEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<SearchEnrollmentsQuery, Result<PagedData<EnrollmentCardDto>>>
{
    public async Task<Result<PagedData<EnrollmentCardDto>>> Handle(
        SearchEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await enrollmentRepository.SearchAsync(request.Term, request.Page, cancellationToken);
        return Result<PagedData<EnrollmentCardDto>>.Success(result, OperationType.Get.ToString());
    }
}

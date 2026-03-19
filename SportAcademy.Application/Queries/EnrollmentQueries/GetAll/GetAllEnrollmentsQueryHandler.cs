using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAll
{
    public class GetAllEnrollmentsQueryHandler(
        IEnrollmentRepository enrollmentRepository)
        : IRequestHandler<GetAllEnrollmentsQuery, Result<PagedData<EnrollmentCardDto>>>
    {
        public async Task<Result<PagedData<EnrollmentCardDto>>> Handle(
            GetAllEnrollmentsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await enrollmentRepository.GetAllAsync(request.Page, cancellationToken);
            return Result<PagedData<EnrollmentCardDto>>.Success(result, OperationType.GetAll.ToString());
        }
    }
}

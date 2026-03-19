using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAll
{
    public record GetAllEnrollmentsQuery(PageRequest Page) : IRequest<Result<PagedData<EnrollmentCardDto>>>;
}

using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForAllSports;

public record GetAllEnrollmentsForAllSportsQuery(
    DateTime? From,
    DateTime? To,
    PageRequest Page
    )
    : IRequest<Result<PagedData<EnrollmentsSportsDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = Page;
}

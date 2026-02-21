using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForSport
{
    public record GetAllEnrollmentsForSportQuery(
        int SportId,
        DateTime? From,
        DateTime? To,
        PageRequest Page
    ) : IRequest<Result<EnrollmentsSportDto>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}

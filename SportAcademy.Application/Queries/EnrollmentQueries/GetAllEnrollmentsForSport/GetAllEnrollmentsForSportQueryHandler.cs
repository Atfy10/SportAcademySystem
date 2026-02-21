using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForSport
{
    public class GetAllEnrollmentsForSportQueryHandler : IRequestHandler<GetAllEnrollmentsForSportQuery, Result<EnrollmentsSportDto>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public GetAllEnrollmentsForSportQueryHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<EnrollmentsSportDto>> Handle(GetAllEnrollmentsForSportQuery request, CancellationToken cancellationToken)
        {
             var enrollments = await _enrollmentRepository.GetAllEnrollmentsForSport(
                request.Page,
                request.From,
                request.To,
                request.SportId,
                cancellationToken);

            return Result<EnrollmentsSportDto>.Success(enrollments, nameof(GetAllEnrollmentsForSportQuery));
        }
    }
}

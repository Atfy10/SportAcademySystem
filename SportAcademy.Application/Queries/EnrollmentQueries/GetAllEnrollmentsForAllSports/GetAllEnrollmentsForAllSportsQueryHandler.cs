using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForAllSports
{
    public class GetAllEnrollmentsForAllSportsQueryHandler : IRequestHandler<GetAllEnrollmentsForAllSportsQuery, Result<PagedData<EnrollmentsSportsDto>>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public GetAllEnrollmentsForAllSportsQueryHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<PagedData<EnrollmentsSportsDto>>> Handle(GetAllEnrollmentsForAllSportsQuery request, CancellationToken ct)
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsForAllSports(
                request.Page,
                request.From,
                request.To,
                ct);

            return Result<PagedData<EnrollmentsSportsDto>>.Success(enrollments, nameof(GetAllEnrollmentsForAllSportsQuery));
        }
    }
}

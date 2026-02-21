using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSport
{
    public class GetEnrollmentsCountForSportQueryHandler : IRequestHandler<GetEnrollmentsCountForSportQuery, Result<int>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public GetEnrollmentsCountForSportQueryHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<int>> Handle(GetEnrollmentsCountForSportQuery request, CancellationToken cancellationToken)
        {
            var enrollmentsCount = await _enrollmentRepository.GetEnrollmentsCountForSport(request.SportId, request.From, request.To, cancellationToken);
            
            return Result<int>.Success(enrollmentsCount, nameof(GetEnrollmentsCountForSportQuery));
        }
    }
}

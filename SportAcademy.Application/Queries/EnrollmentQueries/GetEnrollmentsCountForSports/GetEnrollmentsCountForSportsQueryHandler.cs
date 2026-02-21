using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSports;

public class GetEnrollmentsCountForSportsQueryHandler : IRequestHandler<GetEnrollmentsCountForSportsQuery, Result<int>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetEnrollmentsCountForSportsQueryHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<Result<int>> Handle(GetEnrollmentsCountForSportsQuery request, CancellationToken cancellationToken)
    {
        var enrollmentsCount = await _enrollmentRepository.GetEnrollmentsCountForSports(
            request.From, request.To, cancellationToken);

        return Result<int>.Success(enrollmentsCount, nameof(GetEnrollmentsCountForSportsQuery));
    }
}

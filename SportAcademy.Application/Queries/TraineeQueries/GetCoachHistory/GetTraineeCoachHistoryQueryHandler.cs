using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeQueries.GetCoachHistory
{
    public class GetTraineeCoachHistoryQueryHandler : IRequestHandler<GetTraineeCoachHistoryQuery, Result<List<CoachHistoryEntryDto>>>
    {
        private readonly ITraineeCareerEventRepository _careerEventRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeCoachHistoryQueryHandler(ITraineeCareerEventRepository careerEventRepository)
        {
            _careerEventRepository = careerEventRepository;
        }

        public async Task<Result<List<CoachHistoryEntryDto>>> Handle(GetTraineeCoachHistoryQuery request, CancellationToken cancellationToken)
        {
            var events = await _careerEventRepository.GetCoachEventsForTraineeAsync(request.TraineeId, cancellationToken);

            var result = new List<CoachHistoryEntryDto>();

            // SportId can be null (EnrollmentCoachSnapshotHandler leaves it null when the
            // group's coach lookup misses, e.g. a soft-deleted Coach row still referenced by
            // TraineeGroup.CoachId) - grouping those events together by the shared `null` key
            // would chain together coaching stints from genuinely different, unknown sports
            // and compute bogus end dates between them. Fall back to a unique key per event
            // (its own negative Id) so an unknown-sport event never merges with another one.
            foreach (var sportGroup in events.GroupBy(e => e.SportId ?? -e.Id))
            {
                var chain = sportGroup.OrderBy(e => e.EffectiveDate).ToList();

                for (var i = 0; i < chain.Count; i++)
                {
                    var current = chain[i];

                    // Bound 1: the next coach assignment in this sport's chain.
                    DateTime? nextAssignment = i + 1 < chain.Count ? chain[i + 1].EffectiveDate : null;

                    // Bound 2: the enrollment this stint rode along with being closed out
                    // (soft-deleted, suspended, or expired) without a formal reassignment.
                    DateTime? enrollmentClosed = current.Enrollment switch
                    {
                        { IsDeleted: true, DeletedAt: not null } e => e.DeletedAt,
                        { IsActive: false } e => e.UpdatedAt,
                        { ExpiryDate: var exp } e when exp < DateTime.UtcNow => exp,
                        _ => null
                    };

                    DateTime? endDate = new[] { nextAssignment, enrollmentClosed }
                        .Where(d => d.HasValue)
                        .OrderBy(d => d)
                        .FirstOrDefault();

                    var durationDays = endDate.HasValue
                        ? (int)(endDate.Value - current.EffectiveDate).TotalDays
                        : (int)(DateTime.UtcNow - current.EffectiveDate).TotalDays;

                    result.Add(new CoachHistoryEntryDto(
                        current.CoachId ?? 0,
                        current.Coach?.Employee is { } employee ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
                        current.SportId ?? 0,
                        current.Sport?.Name ?? string.Empty,
                        current.TraineeGroup?.Name ?? string.Empty,
                        current.EffectiveDate,
                        endDate,
                        durationDays,
                        endDate is null,
                        current.EnrollmentId ?? 0));
                }
            }

            return Result<List<CoachHistoryEntryDto>>.Success(
                result.OrderByDescending(r => r.StartDate).ToList(), _operationType);
        }
    }
}

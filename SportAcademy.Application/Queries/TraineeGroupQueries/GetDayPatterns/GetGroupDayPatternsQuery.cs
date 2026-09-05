using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetDayPatterns;

// The weekly day-patterns actually being run for a sport at a branch - what the subscription
// form offers, since the end date is counted across whichever pattern is picked. Offering only
// patterns that exist (rather than free day-picking) is what guarantees a matching group can
// be found when it's time to assign one.
public record GetGroupDayPatternsQuery(
    int SportId,
    int BranchId,
    TraineeGroupType? GroupType = null
) : IRequest<Result<List<GroupDayPatternDto>>>;

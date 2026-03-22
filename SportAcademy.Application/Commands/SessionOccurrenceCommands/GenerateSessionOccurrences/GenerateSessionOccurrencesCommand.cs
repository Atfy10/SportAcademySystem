using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.GenerateSessionOccurrences;

public record GenerateSessionOccurrencesCommand(
    int TraineeGroupId,
    int DurationInDays,
    int? GroupScheduleId,
    DateOnly? StartDate
) : IRequest<Result<int>>;

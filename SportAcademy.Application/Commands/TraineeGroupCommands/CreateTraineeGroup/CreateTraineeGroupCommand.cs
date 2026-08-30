using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup
{
    public record CreateTraineeGroupCommand(
        string? Name,
        SkillLevel SkillLevel,
        int? MaximumCapacity,
        int? DurationInMinutes,
        TraineeGroupGender Gender,
        int BranchId,
        int CoachId,
        List<CreateGroupScheduleSlot> Schedules,
        string? NameAr = null
    ) : IRequest<Result<int>>;

    // StartTime is a plain "HH:mm" string, not TimeOnly, matching how time-of-day values are
    // passed elsewhere in this API (e.g. MarkAttendanceCommand.CheckInTime) - parsed in the handler.
    public record CreateGroupScheduleSlot(DayOfWeek Day, string StartTime);
}

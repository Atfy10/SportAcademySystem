using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup
{
    public record CreateTraineeGroupCommand(
        SkillLevel SkillLevel,
        int? MaximumCapacity,
        int? DurationInMinutes,
        Gender Gender,
        int BranchId,
        int CoachId
    ) : IRequest<Result<int>>;
}

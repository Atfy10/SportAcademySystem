using MediatR;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public record UpdateTraineeGroupCommand(
        int Id,
        SkillLevel SkillLevel,
        int? MaximumCapacity,
        int? DurationInMinutes,
        Gender? Gender,
        int BranchId,
        int CoachId
    ) : IRequest<Result<TraineeGroupDto>>;
}

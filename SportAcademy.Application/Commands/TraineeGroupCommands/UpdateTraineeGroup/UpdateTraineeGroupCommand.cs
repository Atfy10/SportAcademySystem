using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    // No BranchId here - a group's branch can't be changed after creation (see
    // TraineeGroupMapper.ApplyUpdate), and the edit UI never sends one.
    public record UpdateTraineeGroupCommand(
        int Id,
        SkillLevel SkillLevel,
        int? MaximumCapacity,
        int? DurationInMinutes,
        TraineeGroupGender? Gender,
        int CoachId,
        TraineeGroupType? Type = null,
        string? Name = null,
        string? NameAr = null
    ) : IRequest<Result<TraineeGroupDto>>;
}

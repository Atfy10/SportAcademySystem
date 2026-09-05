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
    //
    // No Type either, for a stronger reason: every subscription enrolled in the group was priced
    // against that type and validated against it (SubscriptionGroupTypeMismatchException), and
    // the two types have different capacity ceilings. Flipping it would leave existing
    // enrollments backed by subscriptions bought for the other product, and could put the group
    // over its own limit. A group that needs to become private is a new group.
    public record UpdateTraineeGroupCommand(
        int Id,
        SkillLevel SkillLevel,
        int? MaximumCapacity,
        int? DurationInMinutes,
        TraineeGroupGender? Gender,
        int CoachId,
        string? Name = null,
        string? NameAr = null
    ) : IRequest<Result<TraineeGroupDto>>;
}

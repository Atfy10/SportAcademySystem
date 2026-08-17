using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for CreateMap<UpdateTraineeGroupCommand, TraineeGroup>() /
    // CreateMap<TraineeGroup, TraineeGroupDto>() in TraineeGroupProfile.cs, used only by
    // UpdateTraineeGroupCommandHandler.
    public static class TraineeGroupMapper
    {
        // BranchId is deliberately never touched here: the edit UI states "branch cannot be
        // changed after creation" and never sends it, but the AutoMapper config this replaces
        // mapped it unconditionally - since the command's BranchId isn't nullable, every save
        // silently reset the group's branch to 0. Group capacity is set here too (as a value,
        // not enforced - enforcement lives in CreateEnrollmentCommandHandler).
        public static void ApplyUpdate(TraineeGroup group, UpdateTraineeGroupCommand cmd)
        {
            group.SkillLevel = cmd.SkillLevel;
            if (cmd.MaximumCapacity.HasValue) group.MaximumCapacity = cmd.MaximumCapacity.Value;
            if (cmd.DurationInMinutes.HasValue) group.DurationInMinutes = cmd.DurationInMinutes.Value;
            if (cmd.Gender.HasValue) group.Gender = cmd.Gender.Value;
            group.CoachId = cmd.CoachId;
        }

        public static TraineeGroupDto ToDto(TraineeGroup group) => new(
            group.Id,
            group.SkillLevel,
            group.MaximumCapacity,
            group.DurationInMinutes,
            group.Gender,
            group.BranchId,
            group.CoachId
        );
    }
}

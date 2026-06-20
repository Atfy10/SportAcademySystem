using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class TraineeGroupMappings
    {
        public static TraineeGroup ToTraineeGroup(this CreateTraineeGroupCommand cmd)
        {
            return TraineeGroup.Create(
                cmd.Name,
                cmd.SkillLevel,
                cmd.MaximumCapacity ?? 15,
                cmd.DurationInMinutes ?? 55,
                cmd.Gender,
                cmd.BranchId,
                cmd.CoachId);
        }

        public static void ApplyUpdate(this TraineeGroup traineeGroup, UpdateTraineeGroupCommand cmd)
        {
            traineeGroup.Update(
                cmd.SkillLevel,
                cmd.MaximumCapacity ?? 15,
                cmd.DurationInMinutes ?? 55,
                cmd.Gender ?? traineeGroup.Gender,
                cmd.BranchId,
                cmd.CoachId);
        }

        public static TraineeGroupDto ToDto(this TraineeGroup traineeGroup)
        {
            return new TraineeGroupDto(
                traineeGroup.Id,
                traineeGroup.SkillLevel,
                traineeGroup.MaximumCapacity,
                traineeGroup.DurationInMinutes,
                traineeGroup.Gender,
                traineeGroup.BranchId,
                traineeGroup.CoachId);
        }
    }
}

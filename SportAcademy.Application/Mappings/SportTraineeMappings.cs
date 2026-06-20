using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings
{
    public static class SportTraineeMappings
    {
        public static SportTrainee ToSportTrainee(this CreateSportTraineeCommand cmd)
            => SportTrainee.Create(cmd.SportId, cmd.TraineeId, Enum.Parse<SkillLevel>(cmd.SkillLevel));

        public static SportTrainee ApplyUpdate(this SportTrainee entity, UpdateSportTraineeCommand cmd)
        {
            entity.UpdateSkillLevel(Enum.Parse<SkillLevel>(cmd.SkillLevel));
            return entity;
        }

        public static SportTraineeDto ToDto(this SportTrainee entity)
            => new()
            {
                SportId = entity.SportId,
                TraineeId = entity.TraineeId,
                SkillLevel = entity.SkillLevel.ToString(),
                SportName = entity.Sport?.Name ?? string.Empty,
                TraineeName = entity.Trainee != null
                    ? $"{entity.Trainee.FirstName} {entity.Trainee.LastName}"
                    : string.Empty
            };
    }
}

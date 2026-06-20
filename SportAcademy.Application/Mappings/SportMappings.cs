using SportAcademy.Application.Commands.SportCommands.CreateSport;
using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class SportMappings
    {
        public static Sport ToSport(this CreateSportCommand cmd)
        {
            return Sport.Create(
                cmd.Name,
                cmd.Description,
                cmd.Category,
                cmd.IsRequireHealthTest);
        }

        public static void ApplyUpdate(this Sport sport, UpdateSportCommand cmd)
        {
            sport.Update(
                cmd.Name,
                cmd.Description,
                cmd.Category,
                cmd.IsRequireHealthTest);
        }

        public static SportDto ToDto(this Sport sport)
        {
            return new SportDto(
                sport.Id,
                sport.Name,
                sport.Description,
                sport.Category,
                sport.IsRequireHealthTest);
        }
    }
}

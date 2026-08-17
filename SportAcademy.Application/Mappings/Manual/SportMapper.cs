using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for the UpdateSportCommand <-> Sport AutoMapper config in
    // SportProfile.cs, used only by UpdateSportCommandHandler.
    public static class SportMapper
    {
        public static void ApplyUpdate(Sport sport, UpdateSportCommand cmd)
        {
            sport.Name = cmd.Name;
            sport.Description = cmd.Description;
            sport.Category = cmd.Category;
            sport.IsRequireHealthTest = cmd.IsRequireHealthTest;
        }

        public static SportDto ToDto(Sport sport) => new(
            sport.Id,
            sport.Name,
            sport.Description,
            sport.Category,
            sport.IsRequireHealthTest
        );
    }
}

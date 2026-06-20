using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class CoachMappings
    {
        public static Coach ToCoach(this CreateCoachCommand cmd)
        {
            return Coach.Create(
                cmd.EmployeeId,
                cmd.SportId,
                cmd.SkillLevel);
        }

        public static Coach ToCoach(this CreateCoachWithEmployeeCommand cmd, int employeeId)
        {
            return Coach.Create(
                employeeId,
                cmd.SportId,
                cmd.SkillLevel);
        }

        public static CoachDetailsDto ToCoachDetailsDto(this Coach coach)
        {
            return new CoachDetailsDto(
                coach.EmployeeId,
                coach.Employee.FirstName,
                coach.Employee.LastName,
                coach.Employee.Email.ToString(),
                coach.Employee.PhoneNumber,
                coach.Employee.Branch.Name,
                coach.Sport.Name,
                coach.SkillLevel.ToString(),
                null,
                coach.TraineeGroups
                    .SelectMany(tg => tg.Enrollments)
                    .Count(e => e.IsActive && !e.IsDeleted),
                coach.Employee.HireDate,
                coach.Employee.IsWork,
                coach.Rate);
        }
    }
}

using MediatR;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach
{
    public record CreateCoachCommand(
        int SportId,
        SkillLevel SkillLevel,
        int EmployeeId
    ) : IRequest<Result<int>>;
}

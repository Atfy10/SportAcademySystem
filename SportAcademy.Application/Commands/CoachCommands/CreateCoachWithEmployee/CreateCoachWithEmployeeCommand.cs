using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee
{
    public record CreateCoachWithEmployeeCommand(
        SkillLevel SkillLevel,
        int SportId,
        CreateEmployeeDto Employee
    ) : IRequest<Result<int>>;
}

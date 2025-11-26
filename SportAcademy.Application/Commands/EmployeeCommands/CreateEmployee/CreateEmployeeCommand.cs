using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public record CreateEmployeeCommand(
        string FirstName,
        string LastName,
        string SSN,
        decimal Salary,
        Gender Gender,
        DateOnly BirthDate,
        string Address,
        string PhoneNumber,
        string? SecondNumber,
        Position Position,
        int BranchId,
        string? AppUserId) : IRequest<Result<int>>;
}

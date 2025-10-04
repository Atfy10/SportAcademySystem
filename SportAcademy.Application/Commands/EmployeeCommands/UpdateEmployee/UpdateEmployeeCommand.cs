using MediatR;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee
{
    public record UpdateEmployeeCommand(
        int Id,
        string FirstName,
        string LastName,
        decimal Salary,
        string Address,
        string PhoneNumber,
        string? SecondPhoneNumber,
        Position Position,
        int BranchId) : IRequest<Result<EmployeeDto>>;
}

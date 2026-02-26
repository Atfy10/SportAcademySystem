using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public record CreateEmployeeCommand(
        string FirstName,
        string LastName,
        string SSN,
        decimal Salary,
        Gender Gender,
        DateOnly BirthDate,
        string Email,
        string Nationality,
        string Street,
        string City,
        string PhoneNumber,
        string? SecondNumber,
        Position Position,
        int BranchId) : IRequest<Result<int>>;
}

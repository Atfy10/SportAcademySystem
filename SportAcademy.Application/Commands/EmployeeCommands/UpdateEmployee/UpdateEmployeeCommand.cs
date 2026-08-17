using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee
{
    // Partial update: only Id is required, every other field is optional and left untouched
    // when omitted. The employee list/read DTOs don't currently expose Salary/Street/City, so
    // a frontend edit form can't safely re-submit a full-replacement command without risking
    // blanking data it never had in the first place.
    public record UpdateEmployeeCommand(
        int Id,
        string? FirstName = null,
        string? LastName = null,
        decimal? Salary = null,
        string? Street = null,
        string? City = null,
        string? PhoneNumber = null,
        string? SecondPhoneNumber = null,
        Position? Position = null,
        int? BranchId = null) : IRequest<Result<EmployeeDto>>;
}

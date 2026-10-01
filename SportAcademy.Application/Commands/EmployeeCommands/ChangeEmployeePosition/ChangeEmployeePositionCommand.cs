using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EmployeeCommands.ChangeEmployeePosition
{
    // Its own command (not a field of UpdateEmployeeCommand) because moving someone out of the
    // Coach position has a side effect - their Coach record is hard-deleted - and is refused
    // outright when that isn't safe. See ChangeEmployeePositionCommandHandler.
    public record ChangeEmployeePositionCommand(int Id, Position Position)
        : IRequest<Result<EmployeeDto>>, IRequiresFeature
    {
        public string FeatureKey => "employee-management";
    }
}

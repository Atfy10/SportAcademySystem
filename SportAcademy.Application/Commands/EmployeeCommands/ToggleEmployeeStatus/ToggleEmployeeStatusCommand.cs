using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;

public record ToggleEmployeeStatusCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "employee-management";
}

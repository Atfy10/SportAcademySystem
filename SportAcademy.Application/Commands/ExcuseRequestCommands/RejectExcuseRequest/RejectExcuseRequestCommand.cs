using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.RejectExcuseRequest;

public record RejectExcuseRequestCommand(int Id, string? Note) : IRequest<Result<bool>>;

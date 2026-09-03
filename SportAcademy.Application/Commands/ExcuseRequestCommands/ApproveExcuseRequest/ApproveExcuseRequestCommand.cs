using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.ApproveExcuseRequest;

public record ApproveExcuseRequestCommand(int Id) : IRequest<Result<bool>>;

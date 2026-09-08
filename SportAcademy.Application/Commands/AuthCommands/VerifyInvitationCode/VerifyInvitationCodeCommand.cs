using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.VerifyInvitationCode;

public record VerifyInvitationCodeCommand(string RawToken, string Code) : IRequest<Result>;

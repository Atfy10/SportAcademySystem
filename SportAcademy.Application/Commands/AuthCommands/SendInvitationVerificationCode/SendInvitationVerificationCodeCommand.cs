using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.SendInvitationVerificationCode;

// Anonymous - the invitee has no account yet, only the token from the link they were sent/
// handed. Safe to call repeatedly ("Resend code"): each call replaces whatever code was issued
// before (see Invitation.SetVerificationCode) so only the most recent one is ever valid.
public record SendInvitationVerificationCodeCommand(string RawToken) : IRequest<Result>;

namespace SportAcademy.Application.Interfaces;

// Shared by every caller that actively sends an invitation-related email on demand
// (ResendInvitationCommandHandler, SendInvitationEmailCommandHandler,
// SendInvitationVerificationCodeCommandHandler) so the templates live in exactly one place.
// Deliberately does not swallow send failures itself - unlike the old auto-send-on-create path
// (a courtesy notification nobody explicitly asked for, safe to log-and-continue), every caller
// of this interface is fulfilling an explicit "send this" action the admin/invitee is waiting
// on, so a failure here must surface back to them, not disappear into a log file.
public interface IInvitationEmailSender
{
    Task SendInvitationLinkAsync(string email, string inviteUrl, CancellationToken ct = default);
    Task SendVerificationCodeAsync(string email, string code, CancellationToken ct = default);
}

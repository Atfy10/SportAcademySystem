using SportAcademy.Application.Common.Email;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Services;

public class InvitationEmailSender : IInvitationEmailSender
{
    private readonly IEmailService _emailService;

    public InvitationEmailSender(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task SendInvitationLinkAsync(string email, string inviteUrl, CancellationToken ct = default)
    {
        var body = EmailTemplate.Render(
            preheader: "You've been invited to join AURA Academy.",
            heading: "Welcome to AURA Academy",
            bodyHtml: "<p>You have been invited to set up your organization. Click the button below to accept the invitation and create your account.</p>",
            ctaText: "Accept invitation",
            ctaUrl: inviteUrl,
            footerNote: "This link will expire in 7 days. If you weren't expecting this invitation, you can safely ignore this email.");

        return _emailService.SendAsync(email, "You've been invited to join AURA Academy", body, ct);
    }

    public Task SendVerificationCodeAsync(string email, string code, CancellationToken ct = default)
    {
        var body = EmailTemplate.Render(
            preheader: $"Your verification code is {code}",
            heading: "Verify your email",
            bodyHtml: $"""
                <p>Enter this code to continue setting up your account:</p>
                <p style="margin:20px 0;font-size:32px;font-weight:700;letter-spacing:6px;color:#1F242E;">{code}</p>
                """,
            footerNote: "This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.");

        return _emailService.SendAsync(email, "Your AURA Academy verification code", body, ct);
    }
}

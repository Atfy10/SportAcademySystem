using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.SendOwnerPasswordResetLink;

// Generates an ASP.NET Identity password-reset token and emails it as a link, but does NOT
// consume it here - the owner clicks the link later and lands on a public page that submits
// the token + new password to ResetPasswordCommand, which is what actually consumes it. This
// is the split-apart form of what AdminResetPasswordAsync does in one synchronous call
// (generate then immediately consume) - see IUserRepository for both halves.
public class SendOwnerPasswordResetLinkCommandHandler : IRequestHandler<SendOwnerPasswordResetLinkCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly ILogger<SendOwnerPasswordResetLinkCommandHandler> _logger;
    private readonly string _operation = OperationType.Update.ToString();

    public SendOwnerPasswordResetLinkCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IAppUrlProvider appUrlProvider,
        ILogger<SendOwnerPasswordResetLinkCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _appUrlProvider = appUrlProvider;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendOwnerPasswordResetLinkCommand request, CancellationToken ct)
    {
        var owner = await _userRepository.GetOwnerByIdAsync(request.OwnerUserId, ct);
        if (owner is null)
            return Result<bool>.Failure(_operation, "Owner not found.", 404);

        if (string.IsNullOrWhiteSpace(owner.Email))
            return Result<bool>.Failure(_operation, "Owner has no email address on file.", 400);

        var token = await _userRepository.GeneratePasswordResetTokenAsync(owner);
        var resetUrl = _appUrlProvider.PasswordResetUrl(owner.Id, token);

        var subject = "Reset your AURA Academy password";
        var body = $"""
            <h2>Password Reset Requested</h2>
            <p>A platform administrator requested a password reset for your AURA Academy account.</p>
            <p>Click the link below to set a new password:</p>
            <p><a href="{resetUrl}">{resetUrl}</a></p>
            <p>If you did not expect this, you can safely ignore this email - your password will
            not change unless you open the link above and set a new one.</p>
            """;

        // Token generation + the resulting link are already durable at this point (Identity's
        // reset token is derived from the user's security stamp, not stored separately, and
        // FileLoggingEmailServiceDecorator writes the link to the dev-invitation-links.txt
        // fallback file before ever attempting the real send). A SendGrid failure (bad/expired
        // API key, outage, etc.) must not turn this into a reported failure - the SuperAdmin
        // can still retrieve the link from that file and pass it along manually.
        try
        {
            await _emailService.SendAsync(owner.Email, subject, body, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Failed to email password reset link to owner {OwnerId} ({Email}) - " +
                "the reset link is still valid and was logged to the dev-invitation-links.txt fallback file.",
                owner.Id, owner.Email);
        }

        return Result<bool>.Success(true, _operation);
    }
}

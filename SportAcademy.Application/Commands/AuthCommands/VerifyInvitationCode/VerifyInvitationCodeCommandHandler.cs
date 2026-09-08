using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.VerifyInvitationCode;

public class VerifyInvitationCodeCommandHandler : IRequestHandler<VerifyInvitationCodeCommand, Result>
{
    // A 6-digit code has 1,000,000 possible values - without a cap, an attacker holding a valid
    // invitation link could brute-force it in an unauthenticated loop long before the 10-minute
    // expiry ends the window on its own.
    private const int MaxAttempts = 5;

    private readonly IInvitationRepository _invitationRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public VerifyInvitationCodeCommandHandler(
        IInvitationRepository invitationRepository,
        IInvitationTokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyInvitationCodeCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashToken(request.RawToken);
        var invitation = await _invitationRepository.FindByTokenHashAsync(tokenHash, ct);
        if (invitation is null)
            return Result.Failure(_operation, "Invalid invitation.", 404);

        if (invitation.Status is not InvitationStatus.Pending || invitation.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(_operation, "This invitation is no longer valid.", 400);

        if (invitation.IsEmailVerified)
            return Result.Success(_operation, "Email already verified.");

        if (invitation.VerificationCodeHash is null || invitation.VerificationCodeExpiresAt < DateTime.UtcNow)
            return Result.Failure(_operation, "This code has expired. Request a new one.", 400);

        if (invitation.VerificationCodeAttempts >= MaxAttempts)
            return Result.Failure(_operation, "Too many incorrect attempts. Request a new code.", 400);

        var submittedHash = _tokenService.HashToken(request.Code);
        if (submittedHash != invitation.VerificationCodeHash)
        {
            invitation.VerificationCodeAttempts++;
            await _unitOfWork.SaveChangesAsync(ct);

            var remaining = MaxAttempts - invitation.VerificationCodeAttempts;
            return Result.Failure(
                _operation,
                remaining > 0 ? $"Incorrect code. {remaining} attempt(s) remaining." : "Too many incorrect attempts. Request a new code.",
                400);
        }

        invitation.MarkEmailVerified();
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Email verified.");
    }
}

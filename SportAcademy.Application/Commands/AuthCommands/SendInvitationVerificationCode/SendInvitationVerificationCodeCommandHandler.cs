using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.SendInvitationVerificationCode;

public class SendInvitationVerificationCodeCommandHandler : IRequestHandler<SendInvitationVerificationCodeCommand, Result>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    private readonly IInvitationRepository _invitationRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvitationEmailSender _emailSender;
    private readonly string _operation = OperationType.Add.ToString();

    public SendInvitationVerificationCodeCommandHandler(
        IInvitationRepository invitationRepository,
        IInvitationTokenService tokenService,
        IUnitOfWork unitOfWork,
        IInvitationEmailSender emailSender)
    {
        _invitationRepository = invitationRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }

    public async Task<Result> Handle(SendInvitationVerificationCodeCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashToken(request.RawToken);
        var invitation = await _invitationRepository.FindByTokenHashAsync(tokenHash, ct);
        if (invitation is null)
            return Result.Failure(_operation, "Invalid invitation.", 404);

        if (invitation.Status is not InvitationStatus.Pending || invitation.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(_operation, "This invitation is no longer valid.", 400);

        var code = _tokenService.GenerateNumericCode();
        var codeHash = _tokenService.HashToken(code);
        invitation.SetVerificationCode(codeHash, DateTime.UtcNow.Add(CodeLifetime));
        await _unitOfWork.SaveChangesAsync(ct);

        // Explicit action the invitee is actively waiting on - unlike a courtesy notification, a
        // failure here must surface (they have no other way to get this code), so it is
        // deliberately not caught/swallowed.
        await _emailSender.SendVerificationCodeAsync(invitation.Email, code, ct);

        return Result.Success(_operation, "Verification code sent.");
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.SendInvitationEmail;

// The explicit "Send via Email" half of the Copy/Send choice a newly-created invitation's caller
// gets (see CreateInvitationCommandHandler, which no longer auto-emails). Deliberately reuses the
// exact same link the "Copy" choice would have shown - it never rotates the token, unlike
// ResendInvitationCommand (which exists for the different case of an old/expired link needing a
// fresh one).
public class SendInvitationEmailCommandHandler : IRequestHandler<SendInvitationEmailCommand, Result>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IInvitationEmailSender _emailSender;
    private readonly string _operation = OperationType.Add.ToString();

    public SendInvitationEmailCommandHandler(
        IInvitationRepository invitationRepository,
        IInvitationTokenService tokenService,
        IBaseRepository<Tenant, Guid> tenantRepository,
        IAppUrlProvider appUrlProvider,
        IInvitationEmailSender emailSender)
    {
        _invitationRepository = invitationRepository;
        _tokenService = tokenService;
        _tenantRepository = tenantRepository;
        _appUrlProvider = appUrlProvider;
        _emailSender = emailSender;
    }

    public async Task<Result> Handle(SendInvitationEmailCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashToken(request.RawToken);
        var invitation = await _invitationRepository.FindByTokenHashAsync(tokenHash, ct);
        if (invitation is null || invitation.TenantId != request.TenantId)
            return Result.Failure(_operation, "Invitation not found.", 404);

        if (invitation.Status is not InvitationStatus.Pending)
            return Result.Failure(_operation, "This invitation is no longer active.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct)
            ?? throw new InvalidOperationException("Invitation references a tenant that no longer exists.");

        var inviteUrl = _appUrlProvider.InvitationUrl(tenant.Slug, request.RawToken);
        await _emailSender.SendInvitationLinkAsync(invitation.Email, inviteUrl, ct);

        return Result.Success(_operation, "Invitation email sent.");
    }
}

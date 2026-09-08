using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.ResendInvitation;

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand, Result<InvitationResponse>>
{
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IInvitationEmailSender _emailSender;
    private readonly string _operation = OperationType.Add.ToString();

    public ResendInvitationCommandHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        ITenantIdProvider tenantIdProvider,
        IAppUrlProvider appUrlProvider,
        IInvitationEmailSender emailSender)
    {
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _tenantIdProvider = tenantIdProvider;
        _appUrlProvider = appUrlProvider;
        _emailSender = emailSender;
    }

    public async Task<Result<InvitationResponse>> Handle(ResendInvitationCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<InvitationResponse>.Failure(_operation, "Tenant not found.", 404);

        var rawToken = _tokenService.GenerateRawToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var newInvitation = InvitationMapper.ToEntity(
            request.TenantId,
            request.Email,
            request.InvitedByUserId,
            tokenHash,
            expiresAt);

        var oldInvitations = await _invitationRepository.GetPendingByTenantAndEmailAsync(
            request.TenantId, request.Email, ct);

        foreach (var old in oldInvitations)
        {
            old.Revoke();
            old.ReplacedByInvitationId = newInvitation.Id;
        }

        // The caller (typically Super Admin, resending an owner invitation) may belong to a
        // different tenant than request.TenantId. Without switching context here,
        // TenantSaveChangesInterceptor would either stamp the wrong TenantId onto the new
        // invitation or throw when revoking the old one ("Cannot change the TenantId of an
        // existing entity"), since it compares against the caller's own current tenant.
        var previousTenantId = _tenantIdProvider.TenantId;
        _tenantIdProvider.SetTenantId(request.TenantId);
        try
        {
            await _invitationRepository.AddAsync(newInvitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        finally
        {
            _tenantIdProvider.SetTenantId(previousTenantId);
        }

        // Unlike a fresh CreateInvitationCommand (which just hands the link back for the admin
        // to copy or send), Resend's whole purpose is "the invitee needs this again" - it always
        // actively sends. A failure here must surface, not be swallowed: the admin explicitly
        // asked for this and needs to know if it didn't go out.
        var inviteUrl = _appUrlProvider.InvitationUrl(tenant.Slug, rawToken);
        await _emailSender.SendInvitationLinkAsync(request.Email, inviteUrl, ct);

        return Result<InvitationResponse>.Success(newInvitation.ToResponse(inviteUrl), _operation);
    }
}

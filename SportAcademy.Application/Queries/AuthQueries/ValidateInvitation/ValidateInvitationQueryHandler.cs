using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.AuthQueries.ValidateInvitation;

public class ValidateInvitationQueryHandler
    : IRequestHandler<ValidateInvitationQuery, Result<InvitationResponse>>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvitationTokenService _tokenService;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly string _operation = OperationType.Get.ToString();

    public ValidateInvitationQueryHandler(
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IInvitationTokenService tokenService,
        ITenantIdProvider tenantIdProvider)
    {
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<Result<InvitationResponse>> Handle(
        ValidateInvitationQuery request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashToken(request.RawToken);

        var invitation = await _invitationRepository
            .FindByTokenHashAsync(tokenHash, ct);

        if (invitation is null)
            return Result<InvitationResponse>.Failure(
                _operation, "Invitation not found.", 404);

        if (invitation.Status is not InvitationStatus.Pending)
            return Result<InvitationResponse>.Failure(
                _operation, "Invalid invitation.", 400);

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Expire();

            // Anonymous route - no ambient tenant for request middleware to have set, even
            // though this write is scoped to exactly one already-known tenant (the invitation's
            // own). Same pattern as BanOwnerCommandHandler.
            using (_tenantIdProvider.Impersonate(invitation.TenantId))
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }

            return Result<InvitationResponse>.Failure(
                _operation, "Invitation has expired.", 400);
        }

        return Result<InvitationResponse>.Success(
            invitation.ToResponse(), _operation);
    }
}

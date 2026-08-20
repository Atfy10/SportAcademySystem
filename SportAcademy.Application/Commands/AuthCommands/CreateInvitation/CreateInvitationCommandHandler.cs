using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.Commands.AuthCommands.CreateInvitation;

public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<InvitationResponse>>
{
    // Owner and SuperAdmin are deliberately excluded: Owner is unique per tenant (set once
    // at OwnerSetup acceptance) and SuperAdmin is a platform-only role, never assignable
    // within a tenant.
    private static readonly string[] InvitableStaffRoles = ["Admin", "Manager", "Coach", "Accountant", "User"];

    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly string _operation = OperationType.Add.ToString();

    public CreateInvitationCommandHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ITenantIdProvider tenantIdProvider)
    {
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<Result<InvitationResponse>> Handle(CreateInvitationCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<InvitationResponse>.Failure(_operation, "Tenant not found.", 404);

        if (request.Role is null)
        {
            if (tenant.Status is not TenantStatus.PendingSetup)
                return Result<InvitationResponse>.Failure(
                    _operation, "This tenant has already been set up. Specify a role to invite additional staff.", 400);
        }
        else
        {
            if (!InvitableStaffRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
                return Result<InvitationResponse>.Failure(
                    _operation, $"'{request.Role}' is not a role that can be assigned via invitation.", 400);

            if (tenant.Status is not TenantStatus.Active)
                return Result<InvitationResponse>.Failure(
                    _operation, "Staff can only be invited into an active tenant.", 400);

            if (request.Permissions is { Count: > 0 } &&
                request.Permissions.Any(p => !Permissions.All.Contains(p)))
                return Result<InvitationResponse>.Failure(
                    _operation, "One or more requested permissions are not valid.", 400);
        }

        var rawToken = _tokenService.GenerateRawToken();
        var tokenHash = _tokenService.HashToken(rawToken);

        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7);

        var invitation = InvitationMapper.ToEntity(
            request.TenantId,
            request.Email,
            request.InvitedByUserId,
            tokenHash,
            expiresAt,
            request.Role,
            request.Permissions);

        // See ResendInvitationCommandHandler for why this context switch is required: the
        // caller may not belong to request.TenantId (e.g. Super Admin inviting into a tenant
        // they don't themselves belong to), and TenantSaveChangesInterceptor stamps new rows
        // with the caller's own current tenant unless told otherwise.
        var previousTenantId = _tenantIdProvider.TenantId;
        _tenantIdProvider.SetTenantId(request.TenantId);
        try
        {
            await _invitationRepository.AddAsync(invitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        finally
        {
            _tenantIdProvider.SetTenantId(previousTenantId);
        }

        await _mediator.Publish(
            new InvitationCreatedEvent(invitation.Id, rawToken, tenant.Slug, request.Email), ct);

        return Result<InvitationResponse>.Success(invitation.ToResponse(), _operation);
    }
}

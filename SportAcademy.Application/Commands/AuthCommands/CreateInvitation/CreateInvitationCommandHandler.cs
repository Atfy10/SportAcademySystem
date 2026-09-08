using MediatR;
using Microsoft.Extensions.Logging;
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
    private static readonly string[] InvitableStaffRoles = ["Admin", "Employee", "Accountant"];

    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateInvitationCommandHandler> _logger;
    private readonly string _operation = OperationType.Add.ToString();

    public CreateInvitationCommandHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        ITenantIdProvider tenantIdProvider,
        IAppUrlProvider appUrlProvider,
        IUserRepository userRepository,
        IMediator mediator,
        ILogger<CreateInvitationCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _tenantIdProvider = tenantIdProvider;
        _appUrlProvider = appUrlProvider;
        _userRepository = userRepository;
        _mediator = mediator;
        _logger = logger;
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

            // Employee is the only branch-restricted role (see IBranchAccessProvider) - an
            // Employee invite with no branches would accept into a role that can see nothing,
            // which is never what an admin actually wants, so it's rejected outright rather
            // than silently creating a locked-out account.
            if (string.Equals(request.Role, "Employee", StringComparison.OrdinalIgnoreCase) &&
                request.BranchIds is not { Count: > 0 })
                return Result<InvitationResponse>.Failure(
                    _operation, "At least one branch must be selected for an Employee invitation.", 400);
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
            request.Permissions,
            request.BranchIds);

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

        // No longer auto-emailed - the caller gets the link back and explicitly chooses to copy
        // it or send it (SendInvitationEmailCommand), so creating an invitation never blocks on
        // a live call to the email provider. The in-app "an invitation was sent" notice for the
        // tenant's existing Admins/Owners is unrelated to that and still fires - best-effort,
        // since it must never turn a successful invitation into a reported failure.
        try
        {
            var actorName = await _userRepository.GetDisplayNameAsync(request.InvitedByUserId, ct);
            await _mediator.Publish(new InvitationCreatedEvent(invitation.Id, request.Email, actorName), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish the 'invitation sent' notice for invitation {InvitationId}.", invitation.Id);
        }

        var inviteUrl = _appUrlProvider.InvitationUrl(tenant.Slug, rawToken);
        return Result<InvitationResponse>.Success(invitation.ToResponse(inviteUrl), _operation);
    }
}

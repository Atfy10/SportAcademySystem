using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
using RefreshTokenEntity = SportAcademy.Domain.Entities.RefreshToken;

namespace SportAcademy.Application.Commands.AuthCommands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<AuthResponseDto>>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IBaseRepository<RefreshTokenEntity, int> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;
    private readonly IUserBranchAccessRepository _userBranchAccessRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;
    private const string Operation = "Accept";
    private const int RefreshTokenExpiryDays = 7;

    public AcceptInvitationCommandHandler(
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IBaseRepository<Tenant, Guid> tenantRepository,
        IBaseRepository<RefreshTokenEntity, int> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IUserPermissionOverrideRepository userPermissionOverrideRepository,
        IUserBranchAccessRepository userBranchAccessRepository,
        IProfileRepository profileRepository,
        IMediator mediator,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _tenantRepository = tenantRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _userPermissionOverrideRepository = userPermissionOverrideRepository;
        _userBranchAccessRepository = userBranchAccessRepository;
        _profileRepository = profileRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> Handle(AcceptInvitationCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashToken(request.RawToken);

        var invitation = await _invitationRepository.FindByTokenHashAsync(tokenHash, ct);
        if (invitation is null)
            return Result<AuthResponseDto>.Failure(Operation, "Invalid invitation.", 404);

        if (invitation.Status is not InvitationStatus.Pending)
            return Result<AuthResponseDto>.Failure(Operation, "Invalid invitation.", 400);

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Expire();
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<AuthResponseDto>.Failure(Operation, "Invitation has expired.", 400);
        }

        var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId, ct);
        if (tenant is null)
            return Result<AuthResponseDto>.Failure(Operation, "Tenant not found.", 404);

        var isStaffOnboarding = invitation.Purpose == InvitationPurpose.StaffOnboarding;

        if (isStaffOnboarding)
        {
            if (tenant.Status is not TenantStatus.Active)
                return Result<AuthResponseDto>.Failure(Operation, "This tenant is not currently active.", 400);
        }
        else if (tenant.Status is not TenantStatus.PendingSetup)
        {
            return Result<AuthResponseDto>.Failure(Operation, "Tenant is not in a setup state.", 400);
        }

        var role = isStaffOnboarding ? invitation.Role! : "Owner";

        AppUser user;
        string plainRefreshToken;

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var username = invitation.Email.Split('@')[0];

            user = new AppUser
            {
                UserName = username,
                Email = invitation.Email,
                TenantId = invitation.TenantId,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result<AuthResponseDto>.Failure(Operation, string.Join("; ", errors), 400);
            }

            // Every AppUser is expected to have a companion Profile row (avatar/bio, and now
            // onboarding state) - AppDataSeeder creates one for the seeded SuperAdmin/Owner, but
            // this is the actual creation path for every invited staff member, so it must too.
            await _profileRepository.AddAsyncWithoutSave(
                new Profile { AppUserId = user.Id, HasCompletedOnboarding = false }, ct);

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result<AuthResponseDto>.Failure(Operation, $"Failed to assign {role} role.", 400);
            }

            if (isStaffOnboarding && !string.IsNullOrWhiteSpace(invitation.Permissions))
            {
                // Extra grants beyond the invited role's defaults are expressed as Allow
                // overrides in UserPermissionOverride, not AspNetUserClaims - PermissionResolver
                // (the sole authorization source of truth) only ever reads that table.
                var permissionOverrides = invitation.Permissions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => new UserPermissionOverride
                    {
                        UserId = user.Id,
                        TenantId = user.TenantId,
                        Permission = p,
                        Effect = PermissionEffect.Allow,
                    });
                await _userPermissionOverrideRepository.AddRangeAsync(permissionOverrides, ct);
            }

            if (isStaffOnboarding && !string.IsNullOrWhiteSpace(invitation.BranchIds))
            {
                // CreateInvitationCommandHandler already required at least one branch for an
                // Employee invite (the only branch-restricted role) - this is just materializing
                // that choice into the rows IBranchAccessProvider actually reads.
                var branchAccess = invitation.BranchIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(b => new UserBranchAccess
                    {
                        UserId = user.Id,
                        TenantId = user.TenantId,
                        BranchId = int.Parse(b),
                    });
                await _userBranchAccessRepository.AddRangeAsync(branchAccess, ct);
            }

            if (!isStaffOnboarding)
            {
                tenant.OwnerId = user.Id;
                tenant.Status = TenantStatus.Active;
            }

            invitation.Accept();

            plainRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenService.HashToken(plainRefreshToken);

            var refreshTokenEntity = new RefreshTokenEntity
            {
                TokenHash = refreshTokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        var accessToken = await _jwtTokenService.GenerateJwtToken(user, role);

        // Deliberately outside the try/catch above: the transaction is already committed by this
        // point (the new Owner/staff user exists and, for an Owner, the tenant is already
        // Active), so nothing from here on may turn a successful acceptance into a reported
        // failure. RollbackTransactionAsync would find no active transaction and throw its own
        // error on top, and even without that, a failure in a subscriber (e.g.
        // InvitationAcceptedHandler's best-effort "notify the inviter") is not a reason to tell
        // the newly-created user their onboarding failed when their account and tenant
        // activation are already durably saved. Logged and swallowed, never rethrown.
        try
        {
            await _mediator.Publish(new InvitationAcceptedEvent(invitation.Id, user.Id, invitation.InvitedByUserId), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "InvitationAcceptedEvent subscriber failed for Invitation {InvitationId} / User {UserId} - " +
                "acceptance itself already committed successfully.",
                invitation.Id, user.Id);
        }

        return Result<AuthResponseDto>.Success(
            new AuthResponseDto(accessToken, plainRefreshToken), Operation);
    }
}

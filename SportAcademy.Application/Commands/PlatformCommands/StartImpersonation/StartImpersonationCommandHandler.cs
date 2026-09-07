using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.PlatformCommands.StartImpersonation;

public class StartImpersonationCommandHandler
    : IRequestHandler<StartImpersonationCommand, Result<ImpersonationSessionDto>>
{
    // Hard cap regardless of what a future UI might let someone request - a "temporary" access
    // grant that can be asked for indefinitely isn't temporary.
    private const int MaxDurationMinutes = 60;

    private readonly ITenantRepository _tenantRepository;
    private readonly IImpersonationGrantRepository _grantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMediator _mediator;
    private readonly ILogger<StartImpersonationCommandHandler> _logger;
    private readonly string _operation = OperationType.Add.ToString();

    public StartImpersonationCommandHandler(
        ITenantRepository tenantRepository,
        IImpersonationGrantRepository grantRepository,
        IUserRepository userRepository,
        IUserContextService userContext,
        IJwtTokenService jwtTokenService,
        IMediator mediator,
        ILogger<StartImpersonationCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _grantRepository = grantRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _jwtTokenService = jwtTokenService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<ImpersonationSessionDto>> Handle(
        StartImpersonationCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<ImpersonationSessionDto>.Failure(_operation, "Tenant not found.", 404);

        // Impersonating a tenant that is itself locked out is nonsensical (there is nothing for
        // read-only access to show that the tenant's own users can't already see is blocked) and
        // makes TenantStatusGuardMiddleware's exemption for /api/platform pointless to reason
        // about - the underlying tenant data is still fully readable via /api/platform routes,
        // this just keeps the case from ever coming up.
        if (tenant.Status is not TenantStatus.Active)
            return Result<ImpersonationSessionDto>.Failure(
                _operation, $"Cannot start an impersonation session against a tenant that is {tenant.Status}.", 400);

        var superAdminId = _userContext.UserId
            ?? throw new InvalidOperationException("StartImpersonationCommand ran without an authenticated caller.");
        var superAdmin = await _userRepository.GetByIdAsync(superAdminId, ct)
            ?? throw new IdNotFoundException(nameof(Domain.Entities.AppUser), superAdminId);

        var grant = new TenantImpersonationGrant
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            GrantedByUserId = superAdminId,
            Reason = request.Reason,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(MaxDurationMinutes),
        };
        await _grantRepository.AddAsync(grant, ct);

        var accessToken = await _jwtTokenService.GenerateImpersonationToken(
            superAdmin, request.TenantId, grant.Id, grant.ExpiresAt);

        var session = new ImpersonationSessionDto(
            accessToken, grant.Id, tenant.Id, tenant.DisplayName, grant.ExpiresAt);

        // The grant is already saved and the session token already minted at this point - a
        // failure telling the Owner about it must never block the SuperAdmin from proceeding,
        // the same reasoning as AcceptInvitationCommandHandler's post-commit publish.
        if (tenant.OwnerId is { } ownerId)
        {
            try
            {
                await _mediator.Publish(
                    new ImpersonationStartedEvent(tenant.Id, ownerId, superAdminId, request.Reason, grant.ExpiresAt),
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to notify Owner {OwnerId} of impersonation grant {GrantId} for tenant {TenantId}.",
                    ownerId, grant.Id, tenant.Id);
            }
        }

        return Result<ImpersonationSessionDto>.Success(
            session, _operation, $"Impersonation session started for '{tenant.DisplayName}'.");
    }
}

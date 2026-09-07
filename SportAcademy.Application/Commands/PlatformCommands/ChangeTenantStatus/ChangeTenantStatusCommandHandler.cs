using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;

public class ChangeTenantStatusCommandHandler : IRequestHandler<ChangeTenantStatusCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantStatusCacheInvalidator _tenantStatusCache;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRealtimeService _realtimeService;
    private readonly string _operation = OperationType.Update.ToString();

    public ChangeTenantStatusCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ITenantStatusCacheInvalidator tenantStatusCache,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRealtimeService realtimeService)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _tenantStatusCache = tenantStatusCache;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _realtimeService = realtimeService;
    }

    public async Task<Result> Handle(ChangeTenantStatusCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status == request.NewStatus)
            return Result.Failure(_operation, $"Tenant is already {request.NewStatus}.", 400);

        if (!TenantStatusPolicy.CanTransition(tenant.Status, request.NewStatus))
            return Result.Failure(_operation,
                $"Cannot transition from {tenant.Status} to {request.NewStatus}.", 400);

        request.ResolvedBeforeState = new { tenant.Status };
        tenant.Status = request.NewStatus;
        await _unitOfWork.SaveChangesAsync(ct);

        // So TenantStatusGuardMiddleware sees the new status on the very next request instead
        // of up to the cache's 5-minute sliding window.
        _tenantStatusCache.Invalidate(tenant.Id);

        // Moving away from Active must kill every affected user's live session immediately
        // (F-02), not just leave a held refresh token to fail the next time it's actually
        // presented. Never fires for a transition INTO Active (Suspended/Inactive/Archived ->
        // Active) - a reactivated tenant's users should simply be able to log back in normally.
        if (request.NewStatus != TenantStatus.Active)
        {
            var userIds = await _userRepository.GetUserIdsByTenantIgnoringTenantAsync(tenant.Id, ct);
            await _refreshTokenRepository.RevokeAllTokensForUsersAsync(userIds, ct);

            // Revoking the refresh token only stops a FUTURE token refresh - an already-open
            // SignalR connection (and the still-valid access token behind it) keeps working
            // until it happens to disconnect on its own otherwise. This tells every connected
            // client for this tenant to disconnect and sign out right now.
            await _realtimeService.NotifyTenantStatusChangedAsync(tenant.Id, request.NewStatus.ToString());
        }

        return Result.Success(_operation, $"Tenant status changed to {request.NewStatus}.");
    }
}

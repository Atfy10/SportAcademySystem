using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;

// A thin alias for "transition this tenant to Archived" - not a second rule set. It used to
// have its own two-case guard that disagreed with ChangeTenantStatusCommandHandler's transition
// table (this endpoint allowed Active -> Archived directly; the table didn't), so the same
// state change was legal or illegal depending on which endpoint you called. Both handlers now
// answer to TenantStatusPolicy.CanTransition instead of maintaining their own opinion.
public class ArchiveTenantCommandHandler : IRequestHandler<ArchiveTenantCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantStatusCacheInvalidator _tenantStatusCache;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRealtimeService _realtimeService;
    private readonly string _operation = OperationType.Update.ToString();

    public ArchiveTenantCommandHandler(
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

    public async Task<Result> Handle(ArchiveTenantCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status == TenantStatus.Archived)
            return Result.Failure(_operation, "Tenant is already archived.", 400);

        if (!TenantStatusPolicy.CanTransition(tenant.Status, TenantStatus.Archived))
            return Result.Failure(_operation,
                $"Cannot archive a tenant that is {tenant.Status}. Suspend or deactivate it first.", 400);

        request.ResolvedBeforeState = new { tenant.Status };
        tenant.Status = TenantStatus.Archived;
        await _unitOfWork.SaveChangesAsync(ct);

        _tenantStatusCache.Invalidate(tenant.Id);

        // Belt-and-suspenders alongside ChangeTenantStatusCommandHandler: TenantStatusPolicy only
        // ever reaches Archived from Suspended/Inactive, whose own transition already revoked
        // these same tokens, but revoking again here is a harmless no-op and keeps this handler
        // correct on its own if that ever stops being true.
        var userIds = await _userRepository.GetUserIdsByTenantIgnoringTenantAsync(tenant.Id, ct);
        await _refreshTokenRepository.RevokeAllTokensForUsersAsync(userIds, ct);

        // Same reasoning as ChangeTenantStatusCommandHandler: kill any already-open SignalR
        // connection immediately rather than letting it keep working until it disconnects on
        // its own.
        await _realtimeService.NotifyTenantStatusChangedAsync(tenant.Id, TenantStatus.Archived.ToString());

        return Result.Success(_operation, "Tenant archived successfully.");
    }
}

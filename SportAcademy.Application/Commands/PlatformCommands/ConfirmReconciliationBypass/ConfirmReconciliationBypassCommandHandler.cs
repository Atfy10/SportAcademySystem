using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ConfirmReconciliationBypass;

public class ConfirmReconciliationBypassCommandHandler : IRequestHandler<ConfirmReconciliationBypassCommand, Result>
{
    // Same cap and reasoning as VerifyInvitationCodeCommandHandler: a 6-digit code has 1,000,000
    // possible values - without this, an authenticated-but-not-actually-the-requester caller
    // could brute-force it well within the 10-minute expiry.
    private const int MaxAttempts = 5;

    private readonly ITenantRepository _tenantRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEffectiveLimitService _limitService;
    private readonly IUserContextService _userContext;
    private readonly ITenantStatusCacheInvalidator _tenantStatusCache;
    private readonly IRealtimeService _realtimeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ConfirmReconciliationBypassCommandHandler(
        ITenantRepository tenantRepository,
        IInvitationTokenService tokenService,
        IEffectiveLimitService limitService,
        IUserContextService userContext,
        ITenantStatusCacheInvalidator tenantStatusCache,
        IRealtimeService realtimeService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _limitService = limitService;
        _userContext = userContext;
        _tenantStatusCache = tenantStatusCache;
        _realtimeService = realtimeService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmReconciliationBypassCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status != TenantStatus.PendingLimitSelection)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        var reconciliation = await _tenantRepository.GetOpenReconciliationAsync(request.TenantId, ct);
        if (reconciliation is null)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        if (reconciliation.BypassCodeHash is null || reconciliation.BypassCodeExpiresAt < DateTime.UtcNow)
            return Result.Failure(_operation, "This code has expired. Request a new one.", 400);

        if (reconciliation.BypassCodeAttempts >= MaxAttempts)
            return Result.Failure(_operation, "Too many incorrect attempts. Request a new code.", 400);

        var submittedHash = _tokenService.HashToken(request.Code);
        if (submittedHash != reconciliation.BypassCodeHash)
        {
            reconciliation.BypassCodeAttempts++;
            await _unitOfWork.SaveChangesAsync(ct);

            var remaining = MaxAttempts - reconciliation.BypassCodeAttempts;
            return Result.Failure(
                _operation,
                remaining > 0 ? $"Incorrect code. {remaining} attempt(s) remaining." : "Too many incorrect attempts. Request a new code.",
                400);
        }

        // Recomputed fresh rather than reusing the original RequiredResourcesJson snapshot - a
        // plan/override change since this reconciliation opened could have already resolved (or
        // worsened) the picture, and the bypass response should tell the SuperAdmin what's
        // actually still true, not what was true when the window first opened.
        var stillOver = (await _limitService.GetAllAsync(request.TenantId, ct))
            .Where(l => !l.HasHeadroom && LimitedResources.RequiresSelection(l.ResourceKey))
            .ToDictionary(l => l.ResourceKey, l => l.Used);

        request.ResolvedBeforeState = new { StillOverResources = stillOver };

        tenant.Status = TenantStatus.Active;

        reconciliation.CompletedAt = DateTime.UtcNow;
        reconciliation.CompletedByUserId = _userContext.UserId;
        reconciliation.WasBypassedBySuperAdmin = true;
        reconciliation.BypassCodeHash = null;
        reconciliation.BypassCodeExpiresAt = null;
        reconciliation.BypassCodeAttempts = 0;

        // Commit BEFORE invalidating the cache / notifying - see SubmitLimitSelectionCommandHandler
        // for why the order matters (a concurrent read in the gap can re-cache the stale status
        // for the full sliding TTL).
        await _unitOfWork.SaveChangesAsync(ct);

        _tenantStatusCache.Invalidate(request.TenantId);

        // Same channel SubmitLimitSelectionCommandHandler uses on legitimate completion - the
        // frontend's realtime handler already treats "Active" as "lock lifted", not a logout
        // trigger (see RealtimeContext.tsx).
        await _realtimeService.NotifyTenantStatusChangedAsync(request.TenantId, TenantStatus.Active.ToString());

        var overNote = stillOver.Count > 0
            ? $" This tenant remains over its limit(s): {string.Join(", ", stillOver.Select(kv => $"{kv.Key} ({kv.Value})"))}."
            : "";
        return Result.Success(_operation, $"Tenant force-reactivated without completing its selection.{overNote}");
    }
}

using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.SubmitLimitSelection;

public class SubmitLimitSelectionCommandHandler : IRequestHandler<SubmitLimitSelectionCommand, Result>
{
    private readonly IUserContextService _userContext;
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ISportRepository _sportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEffectiveLimitService _limitService;
    private readonly ILimitSelectionApplier _limitSelectionApplier;
    private readonly ITenantStatusCacheInvalidator _tenantStatusCache;
    private readonly IRealtimeService _realtimeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public SubmitLimitSelectionCommandHandler(
        IUserContextService userContext,
        ITenantRepository tenantRepository,
        IBranchRepository branchRepository,
        ISportRepository sportRepository,
        IUserRepository userRepository,
        IEffectiveLimitService limitService,
        ILimitSelectionApplier limitSelectionApplier,
        ITenantStatusCacheInvalidator tenantStatusCache,
        IRealtimeService realtimeService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
        _sportRepository = sportRepository;
        _userRepository = userRepository;
        _limitService = limitService;
        _limitSelectionApplier = limitSelectionApplier;
        _tenantStatusCache = tenantStatusCache;
        _realtimeService = realtimeService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitLimitSelectionCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "No tenant context.", 400);

        request.ResolvedTenantId = tenantId;

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status != TenantStatus.PendingLimitSelection)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        var reconciliation = await _tenantRepository.GetOpenReconciliationAsync(tenantId.Value, ct);
        if (reconciliation is null)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        // Every selected id must actually belong to this tenant - the global tenant-scoping
        // query filter already confines GetAllAsync/AreIdsExistAsync to the caller's own tenant,
        // so this also implicitly rejects an id from a different tenant entirely.
        var allBranchIds = (await _branchRepository.GetAllAsync(ct)).Select(b => b.Id).ToHashSet();
        if (!request.BranchIds.All(allBranchIds.Contains))
            return Result.Failure(_operation, "One or more selected branches do not belong to this tenant.", 400);

        if (request.SportIds.Count > 0 && !await _sportRepository.AreIdsExistAsync(request.SportIds, ct))
            return Result.Failure(_operation, "One or more selected sports do not belong to this tenant.", 400);

        var allUserIds = (await _userRepository.GetAllAsync(ct)).Select(u => u.Id).ToHashSet();
        if (!request.UserIds.All(allUserIds.Contains))
            return Result.Failure(_operation, "One or more selected users do not belong to this tenant.", 400);

        // Each selection must fit within its own effective cap - the wizard's own live preview
        // should already prevent this client-side, but the server is the actual boundary.
        var limits = (await _limitService.GetAllAsync(tenantId.Value, ct)).ToDictionary(l => l.ResourceKey);
        var overCapErrors = new List<string>();
        CheckSelectionFitsCap(limits, LimitedResources.Branches, request.BranchIds.Count, overCapErrors);
        CheckSelectionFitsCap(limits, LimitedResources.Sports, request.SportIds.Count, overCapErrors);
        CheckSelectionFitsCap(limits, LimitedResources.Users, request.UserIds.Count, overCapErrors);
        if (overCapErrors.Count > 0)
            return Result.Failure(_operation, string.Join(" ", overCapErrors), 400);

        // The retained users must include at least one Owner, or the operator locks themselves
        // (and everyone else) out of the academy permanently - there would be no one left who
        // can invite staff back in, let alone complete a future reconciliation.
        var ownerIds = await _userRepository.GetUserIdsInRolesAsync(["Owner"], ct);
        if (!request.UserIds.Any(ownerIds.Contains))
            return Result.Failure(_operation, "At least one Owner must remain active.", 400);

        request.ResolvedBeforeState = new
        {
            reconciliation.RequiredResourcesJson,
            reconciliation.OpenedAt,
        };

        var applyResult = await _limitSelectionApplier.ApplyAsync(
            tenantId.Value, request.BranchIds, request.SportIds, request.UserIds, ct);

        reconciliation.CompletedAt = DateTime.UtcNow;
        reconciliation.CompletedByUserId = _userContext.UserId;

        tenant.Status = TenantStatus.Active;
        _tenantStatusCache.Invalidate(tenantId.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        // Tells every connected client the lock is lifted - the same channel used to enter
        // PendingLimitSelection in the first place (LimitReconciliationService).
        await _realtimeService.NotifyTenantStatusChangedAsync(tenantId.Value, TenantStatus.Active.ToString());

        var cascadeNote = applyResult.CascadeBannedUsers > 0
            ? $" {applyResult.CascadeBannedUsers} additional user(s) lost access along with their branch and were deactivated too."
            : "";
        return Result.Success(_operation,
            $"Selection applied: {applyResult.DeactivatedBranches} branch(es), {applyResult.DeactivatedSports} sport(s), " +
            $"{applyResult.BannedUsers} user(s) and {applyResult.DeactivatedGroups} group(s) deactivated.{cascadeNote} " +
            "The tenant is active again.");
    }

    private static void CheckSelectionFitsCap(
        Dictionary<string, EffectiveLimit> limits, string resourceKey, int selectedCount, List<string> errors)
    {
        if (limits.TryGetValue(resourceKey, out var limit) && limit.MaxCount is { } max && selectedCount > max)
            errors.Add($"Your plan allows {max} {resourceKey} but {selectedCount} were selected.");
    }
}

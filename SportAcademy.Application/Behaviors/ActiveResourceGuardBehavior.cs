using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Behaviors
{
    // Rejects a command that assigns/creates something at a branch or under a sport that's been
    // deactivated - before the handler runs any business logic. Distinct from
    // BranchAccessValidationBehavior (which governs whether the ACTING USER can reach a branch at
    // all, an orthogonal concern) - this instead governs whether the branch/sport ITSELF is still
    // usable by anyone, regardless of who's asking. A tenant that downgraded its plan and was
    // forced to deselect branches/sports (PLAN_LIMITS_DESIGN.md D1/D5) would otherwise be able to
    // keep creating new records against the deselected ones, defeating the whole point of the
    // forced selection. Deliberately opt-in per request (IRequiresActiveBranch and friends), never
    // applied to a removal/deactivation command - freeing something up must always be allowed,
    // mirroring ToggleBranchStatusCommandHandler's own "deactivating always frees a slot, never
    // blocked" reasoning.
    public class ActiveResourceGuardBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private const string InactiveBranchCode = "BRANCH_INACTIVE";
        private const string InactiveSportCode = "SPORT_INACTIVE";

        private readonly IBranchRepository _branchRepository;
        private readonly ISportRepository _sportRepository;
        private readonly ILogger<ActiveResourceGuardBehavior<TRequest, TResponse>> _logger;

        public ActiveResourceGuardBehavior(
            IBranchRepository branchRepository,
            ISportRepository sportRepository,
            ILogger<ActiveResourceGuardBehavior<TRequest, TResponse>> logger)
        {
            _branchRepository = branchRepository;
            _sportRepository = sportRepository;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = request.GetType().Name;

            int? branchId = request switch
            {
                IRequiresActiveBranch b => b.BranchId,
                IRequiresActiveOptionalBranch ob => ob.BranchId,
                _ => null,
            };

            if (branchId is { } bId)
            {
                var branch = await _branchRepository.GetByIdAsync(bId, cancellationToken);
                if (branch is null || !branch.IsActive)
                {
                    _logger.LogWarning(
                        "Blocked {RequestType}: branch {BranchId} is deactivated or does not exist.",
                        requestType, bId);
                    return ResultFactory.CreateFailureWithCode<TResponse>(
                        requestType, "This branch has been deactivated and can no longer be used.", 400, InactiveBranchCode);
                }
            }

            if (request is IRequiresActiveSport s)
            {
                if (await CheckSportActiveAsync(s.SportId, requestType, cancellationToken) is { } failure)
                    return failure;
            }

            if (request is IRequiresActiveOptionalSport os && os.SportId is { } osId)
            {
                if (await CheckSportActiveAsync(osId, requestType, cancellationToken) is { } failure)
                    return failure;
            }

            if (request is IRequiresActiveSports ss && ss.SportIds is { } sportIds)
            {
                foreach (var sportId in sportIds)
                {
                    if (await CheckSportActiveAsync(sportId, requestType, cancellationToken) is { } failure)
                        return failure;
                }
            }

            return await next(cancellationToken);
        }

        // Shared by all three sport-carrying shapes (single required, single optional, a
        // collection) - returns the rejection response to short-circuit with, or null if the
        // sport is fine.
        private async Task<TResponse?> CheckSportActiveAsync(int sportId, string requestType, CancellationToken ct)
        {
            var sport = await _sportRepository.GetByIdAsync(sportId, ct);
            if (sport is not null && sport.IsActive)
                return null;

            _logger.LogWarning(
                "Blocked {RequestType}: sport {SportId} is deactivated or does not exist.",
                requestType, sportId);
            return ResultFactory.CreateFailureWithCode<TResponse>(
                requestType, "This sport has been deactivated and can no longer be used.", 400, InactiveSportCode);
        }
    }
}

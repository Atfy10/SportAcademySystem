using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Behaviors
{
    // Rejects a request whose IConsumesLimit.ResourceKey has no headroom left for the current
    // tenant, before the handler runs any business logic. Mirrors FeatureGateBehavior exactly -
    // same opt-in-per-request shape, same short-circuit when there's no tenant context (platform/
    // SuperAdmin routes), same "one behavior, one concern" reasoning. Registered immediately
    // after FeatureGateBehavior in DependencyInjection.cs: a request gated on both a disabled
    // feature and an exceeded limit reports the feature problem first, since enabling the
    // feature is usually the more fundamental blocker of the two.
    public class LimitGateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private readonly IUserContextService _userContext;
        private readonly IEffectiveLimitService _limitService;
        private readonly ILocalizationService _localizer;
        private readonly ILogger<LimitGateBehavior<TRequest, TResponse>> _logger;

        public LimitGateBehavior(
            IUserContextService userContext,
            IEffectiveLimitService limitService,
            ILocalizationService localizer,
            ILogger<LimitGateBehavior<TRequest, TResponse>> logger)
        {
            _userContext = userContext;
            _limitService = limitService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IConsumesLimit gated)
                return await next(cancellationToken);

            var requestType = request.GetType().Name;
            var tenantId = _userContext.TenantId;

            if (tenantId is null)
            {
                // No tenant context (e.g. platform/SuperAdmin routes) - limit gating doesn't
                // apply; let the request through to its normal auth checks.
                return await next(cancellationToken);
            }

            var limit = await _limitService.GetAsync(tenantId.Value, gated.ResourceKey, cancellationToken);

            // !HasHeadroom already covers "at or over the cap" (Used >= MaxCount), which is the
            // right threshold for a create-time gate - IsOverLimit (Used > MaxCount) alone would
            // let a tenant sitting exactly at their cap create one more.
            if (!limit.HasHeadroom)
            {
                _logger.LogWarning(
                    "Blocked {RequestType} for tenant {TenantId}: resource '{ResourceKey}' is at {Used}/{Max}.",
                    requestType, tenantId, gated.ResourceKey, limit.Used, limit.MaxCount);

                // Shown as-is by the console when it can't pre-warn (an Arabic-speaking academy got
                // this sentence in English before), so it goes out in the request's language.
                var resourceKey = "limits.resource." + limit.ResourceKey;
                var resourceName = _localizer.Exists(resourceKey) ? _localizer[resourceKey] : limit.ResourceKey;
                var message = _localizer["errors.limit.exceeded", limit.MaxCount ?? 0, resourceName, limit.Used];

                return ResultFactory.CreateFailure<TResponse>(
                    requestType, message, 403, limit.ToErrorDictionary());
            }

            return await next(cancellationToken);
        }
    }
}

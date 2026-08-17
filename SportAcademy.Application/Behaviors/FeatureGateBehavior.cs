using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Behaviors
{
    // Rejects requests whose IRequiresFeature.FeatureKey isn't enabled for the current tenant,
    // before the handler runs any business logic. Requests that don't implement
    // IRequiresFeature are unaffected - this is opt-in per command/query, not a blanket gate.
    public class FeatureGateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private const string FeatureDisabledCode = "FEATURE_DISABLED";

        private readonly IUserContextService _userContext;
        private readonly ITenantRepository _tenantRepository;
        private readonly ILogger<FeatureGateBehavior<TRequest, TResponse>> _logger;

        public FeatureGateBehavior(
            IUserContextService userContext,
            ITenantRepository tenantRepository,
            ILogger<FeatureGateBehavior<TRequest, TResponse>> logger)
        {
            _userContext = userContext;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IRequiresFeature gated)
                return await next(cancellationToken);

            var requestType = request.GetType().Name;
            var tenantId = _userContext.TenantId;

            if (tenantId is null)
            {
                // No tenant context (e.g. platform/SuperAdmin routes) - feature gating doesn't
                // apply; let the request through to its normal auth checks.
                return await next(cancellationToken);
            }

            var isEnabled = await _tenantRepository.IsFeatureEnabledAsync(
                tenantId.Value, gated.FeatureKey, cancellationToken);

            if (!isEnabled)
            {
                _logger.LogWarning(
                    "Blocked {RequestType} for tenant {TenantId}: feature '{FeatureKey}' is not enabled.",
                    requestType, tenantId, gated.FeatureKey);

                return ResultFactory.CreateFailureWithCode<TResponse>(
                    requestType,
                    "This feature isn't included in your plan.",
                    403,
                    FeatureDisabledCode);
            }

            return await next(cancellationToken);
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Behaviors
{
    // Rejects a command that targets a branch outside the caller's allowed set (only meaningful
    // for a branch-restricted "Employee" caller - see IBranchAccessProvider) before the handler
    // runs any business logic. The EF query filter on IBranchScoped entities already stops a
    // restricted caller from *reading* another branch's data; this closes the write-side gap it
    // can't cover on its own - nothing about a query filter stops a caller from supplying an
    // arbitrary BranchId on a brand-new row.
    public class BranchAccessValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private const string BranchAccessDeniedCode = "BRANCH_ACCESS_DENIED";

        private readonly IBranchAccessProvider _branchAccessProvider;
        private readonly ILogger<BranchAccessValidationBehavior<TRequest, TResponse>> _logger;

        public BranchAccessValidationBehavior(
            IBranchAccessProvider branchAccessProvider,
            ILogger<BranchAccessValidationBehavior<TRequest, TResponse>> logger)
        {
            _branchAccessProvider = branchAccessProvider;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Same short-circuit order as FeatureGateBehavior: resolve nothing about the caller
            // until we know the request even carries a BranchId to check. Every query in the
            // app (list retrievals included) flows through this behavior too - only a command
            // that actually implements one of these two interfaces should ever have a reason to
            // touch IBranchAccessProvider at all.
            int? targetBranchId = request switch
            {
                IBranchScopedRequest scoped => scoped.BranchId,
                IOptionallyBranchScopedRequest optional => optional.BranchId,
                _ => null,
            };

            if (targetBranchId is null)
                return await next(cancellationToken);

            if (!_branchAccessProvider.IsRestricted)
                return await next(cancellationToken);

            if (targetBranchId is { } branchId && !_branchAccessProvider.AllowedBranchIds.Contains(branchId))
            {
                var requestType = request.GetType().Name;
                _logger.LogWarning(
                    "Blocked {RequestType}: branch {BranchId} is not in the caller's allowed branches.",
                    requestType, branchId);

                return ResultFactory.CreateFailureWithCode<TResponse>(
                    requestType, "You do not have access to this branch.", 403, BranchAccessDeniedCode);
            }

            return await next(cancellationToken);
        }
    }
}

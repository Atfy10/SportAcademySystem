using MediatR;
using SportAcademy.Application.Common.Pagination;

namespace SportAcademy.Application.Behaviors
{
    public class PaginationNormalizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IPaginatedRequest
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            request.Page = PageRequest.Create(
                request.Page.Page, request.Page.PageSize);

            return await next(cancellationToken);
        }
    }
}

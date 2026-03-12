using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Search;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;

namespace SportAcademy.Application.Behaviors
{
    public class SearchValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ISearchRequest
        where TResponse : Result
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Term))
                return (TResponse)Result.Failure("search", "Search term required");

            if (request.Term.Trim().Length < 2)
                return (TResponse)Result.Failure("search", "Minimum 2 characters");

            return await next(cancellationToken);

        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.ExcuseRequestQueries.CountPending;

public record CountPendingExcuseRequestsQuery : IRequest<Result<int>>;

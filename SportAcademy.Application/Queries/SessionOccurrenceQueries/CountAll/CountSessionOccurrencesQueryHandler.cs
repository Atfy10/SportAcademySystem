using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.CountAll;

public class CountSessionOccurrencesQueryHandler(
    ISessionOccurrenceRepository sessionOccurrenceRepository)
    : IRequestHandler<CountSessionOccurrencesQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        CountSessionOccurrencesQuery request,
        CancellationToken cancellationToken)
    {
        var count = await sessionOccurrenceRepository.CountAsync(cancellationToken);
        return Result<int>.Success(count, OperationType.GetAll.ToString());
    }
}

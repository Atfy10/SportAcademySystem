using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.SearchSessionOccurrences;

public class SearchSessionOccurrencesQueryHandler : IRequestHandler<SearchSessionOccurrencesQuery, Result<PagedData<SessionOccurrenceDto>>>
{
    private readonly ISessionOccurrenceRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public SearchSessionOccurrencesQueryHandler(ISessionOccurrenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedData<SessionOccurrenceDto>>> Handle(SearchSessionOccurrencesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.SearchAsync(request.Term, request.Page, cancellationToken);
        return Result<PagedData<SessionOccurrenceDto>>.Success(result, _operation);
    }
}

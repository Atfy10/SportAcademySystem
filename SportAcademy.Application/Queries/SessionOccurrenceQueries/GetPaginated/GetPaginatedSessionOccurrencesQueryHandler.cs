using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetPaginated;

public class GetPaginatedSessionOccurrencesQueryHandler : IRequestHandler<GetPaginatedSessionOccurrencesQuery, Result<PagedData<SessionOccurrenceDto>>>
{
    private readonly ISessionOccurrenceRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetPaginatedSessionOccurrencesQueryHandler(ISessionOccurrenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedData<SessionOccurrenceDto>>> Handle(GetPaginatedSessionOccurrencesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllPaginatedAsync(request.Page, cancellationToken);
        return Result<PagedData<SessionOccurrenceDto>>.Success(result, _operation);
    }
}

public class GetSessionOccurrencesByDateQueryHandler : IRequestHandler<GetSessionOccurrencesByDateQuery, Result<PagedData<SessionOccurrenceDto>>>
{
    private readonly ISessionOccurrenceRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetSessionOccurrencesByDateQueryHandler(ISessionOccurrenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedData<SessionOccurrenceDto>>> Handle(GetSessionOccurrencesByDateQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetByDateAsync(request.Date, request.Page, cancellationToken);
        return Result<PagedData<SessionOccurrenceDto>>.Success(result, _operation);
    }
}

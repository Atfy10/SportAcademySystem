using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.Search;

public class SearchTraineeGroupsQueryHandler : IRequestHandler<SearchTraineeGroupsQuery, Result<PagedData<ListTraineeGroupDto>>>
{
    private readonly ITraineeGroupRepository _traineeGroupRepository;
    private readonly string _operationType = OperationType.GetAll.ToString();

    public SearchTraineeGroupsQueryHandler(ITraineeGroupRepository traineeGroupRepository)
    {
        _traineeGroupRepository = traineeGroupRepository;
    }

    public async Task<Result<PagedData<ListTraineeGroupDto>>> Handle(SearchTraineeGroupsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Trim().Length < 2)
        {
            throw new InvalidSearchTermException(2);
        }

        var result = await _traineeGroupRepository.SearchAsync(request.SearchTerm.Trim(), request.Page, cancellationToken);
        return Result<PagedData<ListTraineeGroupDto>>.Success(result, _operationType);
    }
}

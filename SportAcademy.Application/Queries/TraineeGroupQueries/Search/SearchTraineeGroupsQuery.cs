using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Search;
using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.Search;

public record SearchTraineeGroupsQuery(string SearchTerm, PageRequest Page)
    : IRequest<Result<PagedData<ListTraineeGroupDto>>>, IPaginatedRequest, ISearchRequest
{
    public PageRequest Page { get; set; } = Page;
    string ISearchRequest.Term => SearchTerm;
}

using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;

namespace SportAcademy.Application.Queries.FamilyQueries.GetAllFamilies;

public record GetAllFamiliesQuery(PageRequest PageRequest) 
    : IRequest<Result<PagedData<FamilyDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = PageRequest;
}    


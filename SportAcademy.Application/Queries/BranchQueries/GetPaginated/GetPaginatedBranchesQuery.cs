using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;

namespace SportAcademy.Application.Queries.BranchQueries.GetPaginated;

public record GetPaginatedBranchesQuery(PageRequest Page) : IRequest<Result<PagedData<BranchCardDto>>>;

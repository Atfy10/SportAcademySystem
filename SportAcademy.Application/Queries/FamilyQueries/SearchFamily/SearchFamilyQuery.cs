using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Search;
using SportAcademy.Application.DTOs.FamilyDtos;

namespace SportAcademy.Application.Queries.FamilyQueries.SearchFamily;

public record SearchFamilyQuery(string Term)
    : IRequest<Result<IReadOnlyList<FamilyDto>>>, ISearchRequest;
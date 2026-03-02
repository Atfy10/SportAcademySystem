using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;

namespace SportAcademy.Application.Queries.SportQueries.SearchSportsName;

public record SearchSportsNameQuery(string SearchTerm) 
    : IRequest<Result<IReadOnlyList<SportDropDownListDto>>>;

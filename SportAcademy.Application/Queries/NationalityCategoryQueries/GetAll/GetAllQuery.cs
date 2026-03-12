using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityCategoryDtos;

namespace SportAcademy.Application.Queries.NationalityCategoryQueries.GetAll;

public record GetAllQuery() 
    : IRequest<Result<IReadOnlyList<NationalityCategoryDto>>>;
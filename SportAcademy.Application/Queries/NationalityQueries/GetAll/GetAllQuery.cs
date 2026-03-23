using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityDtos;

namespace SportAcademy.Application.Queries.NationalityQueries.GetAll;

public record GetAllQuery() 
    : IRequest<Result<IReadOnlyList<NationalityDto>>>;

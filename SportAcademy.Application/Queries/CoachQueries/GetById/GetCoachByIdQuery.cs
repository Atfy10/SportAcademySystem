using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Queries.CoachQueries.GetById;

public record GetCoachByIdQuery(int Id) : IRequest<Result<CoachDetailsDto>>;

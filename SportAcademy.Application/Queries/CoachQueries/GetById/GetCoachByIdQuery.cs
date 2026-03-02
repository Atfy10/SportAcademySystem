using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.CoachQueries.GetById;

public record GetCoachByIdQuery(int Id) : IRequest<Result<CoachDetailsDto>>;

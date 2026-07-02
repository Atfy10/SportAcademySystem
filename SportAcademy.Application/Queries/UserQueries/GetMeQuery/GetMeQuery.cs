using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;

namespace SportAcademy.Application.Queries.UserQueries.GetMeQuery;

public record GetMeQuery : IRequest<Result<MeResponse>>;

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;

namespace SportAcademy.Application.Queries.UserQueries.GetAll
{
    public record GetAllUsersQuery() : IRequest<Result<List<AppUserCardDto>>>;
}

using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetAll
{
    public record GetAllUsersQuery() : IRequest<Result<List<AppUserDto>>>;
}

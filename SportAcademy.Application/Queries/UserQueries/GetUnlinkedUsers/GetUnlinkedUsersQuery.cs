using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetUnlinkedUsers
{
    public record GetUnlinkedUsersQuery() : IRequest<List<AppUserDto>>;
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<Result<AppUserDto>>;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.SportQueries.GetById
{
    public record GetSportByIdQuery(int Id) : IRequest<Result<SportDto>>;
}

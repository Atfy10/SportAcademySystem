using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetAll
{
    public record GetAllSportsQuery() : IRequest<Result<List<SportDto>>>;
}

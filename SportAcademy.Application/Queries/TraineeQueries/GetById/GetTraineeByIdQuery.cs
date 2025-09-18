using MediatR;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetById
{
    public record GetTraineeByIdQuery(int Id) : IRequest<Result<TraineeDto>>;
}

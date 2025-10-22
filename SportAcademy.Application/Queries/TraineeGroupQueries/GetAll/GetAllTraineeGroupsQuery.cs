using MediatR;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAll
{
    public record GetAllTraineeGroupsQuery() : IRequest<Result<List<TraineeGroupDto>>>;
}

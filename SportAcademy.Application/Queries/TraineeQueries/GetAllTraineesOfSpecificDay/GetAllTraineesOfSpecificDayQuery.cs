using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay
{
    public record GetAllTraineesOfSpecificDayQuery(DateTime Date, PageRequest Page)
        : IRequest<Result<PagedData<TraineeOfSpecificDayDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}

using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAll
{
    public record GetAllTraineeGroupsQuery(PageRequest Page)
        : IRequest<Result<PagedData<TraineeGroupDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}

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
    public record GetAllTraineeGroupsQuery(PageRequest Page, TimeOnly? FromTime = null, TimeOnly? ToTime = null, SportAcademy.Domain.Enums.TraineeGroupType? Type = null)
        : IRequest<Result<PagedData<TraineeGroupCardDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}

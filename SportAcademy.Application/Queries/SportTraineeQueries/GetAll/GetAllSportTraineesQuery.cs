using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetAll
{
	public record GetAllSportTraineesQuery()
		: IRequest<Result<List<SportTraineeDto>>>;

    //public record GetAllSportTraineesQuery(PageRequest Page)
    //: IRequest<Result<PagedData<SportTraineeDto>>>, IPaginatedRequest
    //{
    //    public PageRequest Page { get; set; } = Page;
    //}
}

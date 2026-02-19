using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord
{
    public record GetCoachEmployeesWithoutCoachRecordQuery
        : IRequest<Result<PagedData<EmployeeDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; }

        public GetCoachEmployeesWithoutCoachRecordQuery(PageRequest page)
        {
            Page = page;
        }
    }
}

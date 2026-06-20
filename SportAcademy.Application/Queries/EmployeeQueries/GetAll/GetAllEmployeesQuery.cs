using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetAll
{
    public record GetAllEmployeesQuery(
        PageRequest Page,
        string? Status = null,
        int? BranchId = null,
        string? Position = null,
        string? SortBy = null,
        string? SortOrder = null
    ) : IRequest<Result<PagedData<EmployeeCardDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
        public string? Status { get; set; } = Status;
        public int? BranchId { get; set; } = BranchId;
        public string? Position { get; set; } = Position;
        public string? SortBy { get; set; } = SortBy ?? "name";
        public string? SortOrder { get; set; } = SortOrder ?? "asc";
    }
}

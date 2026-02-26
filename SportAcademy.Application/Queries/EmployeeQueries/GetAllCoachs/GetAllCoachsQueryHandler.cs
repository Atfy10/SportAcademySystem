using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetAllCoachs
{
    public class GetAllCoachsQueryHandler : IRequestHandler<GetAllCoachsQuery, Result<PagedData<CoachCardDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetAllCoachsQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public Task<Result<PagedData<CoachCardDto>>> Handle(GetAllCoachsQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

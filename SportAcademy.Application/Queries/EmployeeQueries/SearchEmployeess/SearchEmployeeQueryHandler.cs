using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using System.Diagnostics;

namespace SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess
{
    public class SearchEmployeeQueryHandler : IRequestHandler<SearchEmployeeQuery, Result<PagedData<EmployeeCardDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public SearchEmployeeQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper
        )
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedData<EmployeeCardDto>>> Handle(SearchEmployeeQuery request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(request.Term))
                return Result<PagedData<EmployeeCardDto>>.Failure(nameof(SearchEmployeeQuery), "Search term required");

            if (request.Term.Trim().Length < 2)
                return Result<PagedData<EmployeeCardDto>>.Failure(nameof(SearchEmployeeQuery), "Minimum 2 characters");

            var employees = await _employeeRepository.SearchAsync(request.SearchTerm, request.Page, cancellationToken);
            //var employeeCardDto = _mapper.Map<List<EmployeeCardDto>>(employees.Items);
            //var employeeCardDtoPag = new PagedData<EmployeeCardDto>
            //{
            //    Items = employeeCardDto,
            //    TotalCount = employees.TotalCount,
            //    Page = employees.Page,
            //    PageSize = employees.PageSize,
            //};

            sw.Stop();

            return Result<PagedData<EmployeeCardDto>>.Success(employees, nameof(SearchEmployeeQuery), $"Search took {sw.ElapsedMilliseconds} ms");
        }
    }
}

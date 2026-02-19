using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;

public record GetActiveCoachesCountQueryHandler : IRequestHandler<GetActiveCoachesCountQuery, Result<int>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetActiveCoachesCountQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<int>> Handle(GetActiveCoachesCountQuery request, CancellationToken ct)
    {
        var count = await _employeeRepository.GetActiveCoachesCountAsync(ct);

        return Result<int>.Success(count, nameof(GetActiveCoachesCountQuery));
    }
}

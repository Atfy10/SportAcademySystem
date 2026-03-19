using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.CoachQueries.GetAllForDropdown;

public class GetAllCoachesForDropdownQueryHandler : IRequestHandler<GetAllCoachesForDropdownQuery, Result<List<CoachDropdownItemDto>>>
{
    private readonly ICoachRepository _coachRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetAllCoachesForDropdownQueryHandler(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
    }

    public async Task<Result<List<CoachDropdownItemDto>>> Handle(GetAllCoachesForDropdownQuery request, CancellationToken cancellationToken)
    {
        var coaches = await _coachRepository.GetAllForDropdownAsync(cancellationToken);
        return Result<List<CoachDropdownItemDto>>.Success(coaches, _operation);
    }
}

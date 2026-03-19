using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAllForDropdown;

public class GetAllTraineesForDropdownQueryHandler : IRequestHandler<GetAllTraineesForDropdownQuery, Result<List<TraineeDropdownDto>>>
{
    private readonly ITraineeRepository _traineeRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetAllTraineesForDropdownQueryHandler(ITraineeRepository traineeRepository)
    {
        _traineeRepository = traineeRepository;
    }

    public async Task<Result<List<TraineeDropdownDto>>> Handle(GetAllTraineesForDropdownQuery request, CancellationToken cancellationToken)
    {
        var trainees = await _traineeRepository.GetAllForDropdownAsync(cancellationToken);
        return Result<List<TraineeDropdownDto>>.Success(trainees, _operation);
    }
}

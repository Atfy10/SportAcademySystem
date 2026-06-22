using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TraineeQueries.ExportTrainees;

public class ExportTraineesQueryHandler
    : IRequestHandler<ExportTraineesQuery, Result<List<TraineeExportDto>>>
{
    private readonly ITraineeRepository _traineeRepository;

    public ExportTraineesQueryHandler(ITraineeRepository traineeRepository)
    {
        _traineeRepository = traineeRepository;
    }

    public async Task<Result<List<TraineeExportDto>>> Handle(
        ExportTraineesQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _traineeRepository.GetExportDataByIdsAsync(
            request.Ids, cancellationToken);

        return Result<List<TraineeExportDto>>.Success(data, "ExportTrainees");
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetAnalysisById;

public class GetAnalysisByIdQueryHandler
    : IRequestHandler<GetAnalysisByIdQuery, Result<VideoAnalysisResultDto>>
{
    private readonly IVideoAnalysisRepository _repository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetAnalysisByIdQueryHandler(
        IVideoAnalysisRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<VideoAnalysisResultDto>> Handle(
        GetAnalysisByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<VideoAnalysisResultDto>.Failure(_operation,
                "Video analysis not found", 404);

        var dto = entity.ToDto();
        return Result<VideoAnalysisResultDto>.Success(dto, _operation);
    }
}

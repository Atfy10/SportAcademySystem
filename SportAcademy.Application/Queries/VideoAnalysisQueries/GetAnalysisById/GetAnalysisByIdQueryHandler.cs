using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetAnalysisById;

public class GetAnalysisByIdQueryHandler
    : IRequestHandler<GetAnalysisByIdQuery, Result<VideoAnalysisResultDto>>
{
    private readonly IVideoAnalysisRepository _repository;
    private readonly IMapper _mapper;
    private readonly string _operation = OperationType.Get.ToString();

    public GetAnalysisByIdQueryHandler(
        IVideoAnalysisRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<VideoAnalysisResultDto>> Handle(
        GetAnalysisByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<VideoAnalysisResultDto>.Failure(_operation,
                "Video analysis not found", 404);

        var dto = _mapper.Map<VideoAnalysisResultDto>(entity);
        return Result<VideoAnalysisResultDto>.Success(dto, _operation);
    }
}

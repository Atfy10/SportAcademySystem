using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetUserAnalyses;

public class GetUserAnalysesQueryHandler
    : IRequestHandler<GetUserAnalysesQuery, Result<List<VideoAnalysisResultDto>>>
{
    private readonly IVideoAnalysisRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetUserAnalysesQueryHandler(
        IVideoAnalysisRepository repository,
        IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<List<VideoAnalysisResultDto>>> Handle(
        GetUserAnalysesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByUserIdAsync(
            _userContext.UserId, cancellationToken);

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return Result<List<VideoAnalysisResultDto>>.Success(dtos, _operation);
    }
}

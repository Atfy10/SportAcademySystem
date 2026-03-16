using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetUserAnalyses;

public class GetUserAnalysesQueryHandler
    : IRequestHandler<GetUserAnalysesQuery, Result<List<VideoAnalysisResultDto>>>
{
    private readonly IVideoAnalysisRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly IMapper _mapper;
    private readonly string _operation = OperationType.Get.ToString();

    public GetUserAnalysesQueryHandler(
        IVideoAnalysisRepository repository,
        IUserContextService userContext,
        IMapper mapper)
    {
        _repository = repository;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<Result<List<VideoAnalysisResultDto>>> Handle(
        GetUserAnalysesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetByUserIdAsync(
            _userContext.UserId, cancellationToken);

        var dtos = _mapper.Map<List<VideoAnalysisResultDto>>(entities);
        return Result<List<VideoAnalysisResultDto>>.Success(dtos, _operation);
    }
}

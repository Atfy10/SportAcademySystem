using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetAnalysisById;

public record GetAnalysisByIdQuery(Guid Id) : IRequest<Result<VideoAnalysisResultDto>>;

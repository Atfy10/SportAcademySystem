using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;

namespace SportAcademy.Application.Queries.VideoAnalysisQueries.GetUserAnalyses;

public record GetUserAnalysesQuery() : IRequest<Result<List<VideoAnalysisResultDto>>>;

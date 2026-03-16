using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;

namespace SportAcademy.Application.Commands.VideoAnalysisCommands.AnalyzeVideo;

public record AnalyzeVideoCommand(
    string MovementType,
    PoseLandmarksDto Landmarks
) : IRequest<Result<VideoAnalysisResultDto>>;

namespace SportAcademy.Application.DTOs.VideoAnalysisDtos;

public record VideoAnalysisResultDto(
    Guid Id,
    string MovementType,
    AiAnalysisResultDto AiAnalysisResult,
    DateTime CreatedAt
);

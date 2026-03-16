using System.Text.Json.Serialization;

namespace SportAcademy.Application.DTOs.VideoAnalysisDtos;

public class AiAnalysisResultDto
{
    [JsonPropertyName("bodyPostureAnalysis")]
    public string BodyPostureAnalysis { get; set; } = string.Empty;

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = [];

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];

    [JsonPropertyName("recommendedExercises")]
    public List<RecommendedExerciseDto> RecommendedExercises { get; set; } = [];

    [JsonPropertyName("suitableSports")]
    public List<SuitableSportDto> SuitableSports { get; set; } = [];

    [JsonPropertyName("generalTips")]
    public List<string> GeneralTips { get; set; } = [];
}

public class RecommendedExerciseDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;
}

public class SuitableSportDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

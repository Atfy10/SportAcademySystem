using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Domain.Entities;
using System.Text.Json;

namespace SportAcademy.Application.Mappings
{
    public static class VideoAnalysisMappings
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static VideoAnalysisResultDto ToDto(this VideoAnalysis entity)
            => new(entity.Id, entity.MovementType, ParseAiResult(entity.AiAnalysisResult), entity.CreatedAt);

        private static AiAnalysisResultDto ParseAiResult(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new AiAnalysisResultDto();

            try
            {
                var result = JsonSerializer.Deserialize<AiAnalysisResultDto>(raw, _jsonOptions);
                if (result is not null)
                    return result;
            }
            catch (JsonException) { }

            return new AiAnalysisResultDto
            {
                BodyPostureAnalysis = raw.Trim()
            };
        }
    }
}

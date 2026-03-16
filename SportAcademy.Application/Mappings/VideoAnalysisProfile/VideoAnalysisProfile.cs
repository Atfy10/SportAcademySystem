using AutoMapper;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Domain.Entities;
using System.Text.Json;

namespace SportAcademy.Application.Mappings.VideoAnalysisProfile;

public class VideoAnalysisProfile : AutoMapper.Profile
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VideoAnalysisProfile()
    {
        CreateMap<VideoAnalysis, VideoAnalysisResultDto>()
            .ForCtorParam("AiAnalysisResult",
                opt => opt.MapFrom(src => ParseAiResult(src.AiAnalysisResult)));
    }

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
        catch (JsonException)
        {
            // Fall through to fallback
        }

        return new AiAnalysisResultDto
        {
            BodyPostureAnalysis = raw.Trim()
        };
    }
}

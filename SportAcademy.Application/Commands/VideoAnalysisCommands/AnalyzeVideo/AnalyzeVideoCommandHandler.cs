using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.VideoAnalysisDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SportAcademy.Application.Commands.VideoAnalysisCommands.AnalyzeVideo;

public class AnalyzeVideoCommandHandler
    : IRequestHandler<AnalyzeVideoCommand, Result<VideoAnalysisResultDto>>
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly IVideoAnalysisRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Add.ToString();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AnalyzeVideoCommandHandler(
        IOpenRouterClient openRouterClient,
        IVideoAnalysisRepository repository,
        IUserContextService userContext)
    {
        _openRouterClient = openRouterClient;
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<VideoAnalysisResultDto>> Handle(
        AnalyzeVideoCommand request,
        CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt();
        var userMessage = BuildUserMessage(request);

        // Call AI for analysis
        var aiResponse = await _openRouterClient.SendAsync(
            systemPrompt, userMessage, cancellationToken);

        // Parse and normalize the AI response to ensure consistent schema
        var parsedResult = ParseAiResponse(aiResponse);
        var normalizedJson = JsonSerializer.Serialize(parsedResult, _jsonOptions);

        // Save to database
        var entity = VideoAnalysis.Create(
            Guid.NewGuid(),
            _userContext.UserId,
            request.MovementType,
            JsonSerializer.Serialize(request.Landmarks),
            normalizedJson);

        await _repository.AddAsync(entity, cancellationToken);

        var dto = new VideoAnalysisResultDto(
            entity.Id,
            entity.MovementType,
            parsedResult,
            entity.CreatedAt
        );
        return Result<VideoAnalysisResultDto>.Success(dto, _operation);
    }

    /// <summary>
    /// Parses the raw AI response string into a structured AiAnalysisResultDto.
    /// Handles markdown code block wrappers and falls back gracefully for non-JSON responses.
    /// </summary>
    private static AiAnalysisResultDto ParseAiResponse(string rawResponse)
    {
        var cleaned = StripMarkdownCodeBlock(rawResponse.Trim());

        try
        {
            var result = JsonSerializer.Deserialize<AiAnalysisResultDto>(cleaned, _jsonOptions);
            if (result is not null)
                return result;
        }
        catch (JsonException)
        {
            // Fall through to fallback
        }

        // Fallback: wrap raw text in the expected structure
        return new AiAnalysisResultDto
        {
            BodyPostureAnalysis = rawResponse.Trim(),
            Strengths = [],
            Weaknesses = [],
            RecommendedExercises = [],
            SuitableSports = [],
            GeneralTips = []
        };
    }

    /// <summary>
    /// Strips markdown code block fences (```json ... ``` or ``` ... ```) from the response.
    /// </summary>
    private static string StripMarkdownCodeBlock(string text)
    {
        var match = Regex.Match(text, @"^```(?:json)?\s*\n(.*?)\n\s*```$", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : text;
    }

    private static string BuildSystemPrompt()
    {
        return """
            You are an expert sports biomechanics analyst and physiotherapist with deep knowledge of kinesiology.
            You analyze body pose landmarks data extracted from video using MediaPipe.
            Your task is to provide a comprehensive, detailed analysis in Arabic language.

            You will receive:
            - Movement type and video metadata
            - Average joint angles across the entire movement
            - Statistical analysis per joint: min, max, range of motion (ROM), and standard deviation
            - Left/Right symmetry comparison for each joint pair
            - Temporal analysis comparing the first and last thirds of the movement

            Use all of this data to provide a thorough analysis. Pay special attention to:
            - Joint angles and bilateral symmetry (left vs right differences > 5° are notable, > 10° are significant)
            - Range of motion for each joint (limited ROM may indicate stiffness or compensation)
            - Standard deviation (high values indicate inconsistency or instability)
            - Temporal changes (fatigue patterns, compensations developing over time)
            - Body alignment and trunk lean (ideal trunk lean during movement is 5-15°)
            - Movement efficiency and coordination
            - Potential injury risks based on asymmetry, excessive angles, or poor mechanics
            - Muscle imbalances suggested by the data

            Your response MUST be ONLY valid JSON in the following format (no markdown, no extra text):
            {
                "bodyPostureAnalysis": "تحليل شامل ومفصل لوضعية الجسم أثناء الحركة مع ذكر الملاحظات الرئيسية",
                "strengths": ["نقطة قوة 1", "نقطة قوة 2", ...],
                "weaknesses": ["نقطة ضعف 1", "نقطة ضعف 2", ...],
                "recommendedExercises": [
                    {"name": "اسم التمرين", "description": "وصف مختصر للتمرين وكيفية أدائه", "purpose": "الهدف من التمرين وعلاقته بالمشكلة"}
                ],
                "suitableSports": [
                    {"name": "اسم الرياضة", "reason": "لماذا هذه الرياضة مناسبة بناءً على التحليل"}
                ],
                "generalTips": ["نصيحة 1", "نصيحة 2", ...]
            }

            Provide at least 3 items for strengths, weaknesses, recommendedExercises, and generalTips.
            Provide at least 2 items for suitableSports.
            """;
    }

    private static string BuildUserMessage(AnalyzeVideoCommand request)
    {
        var sb = new StringBuilder();
        var angles = request.Landmarks.AverageAngles;
        var frames = request.Landmarks.Frames;

        // ── Basic Info ──
        sb.AppendLine($"Movement Type: {request.MovementType}");
        sb.AppendLine($"Video Duration: {request.Landmarks.VideoDurationSeconds:F1} seconds");
        sb.AppendLine($"Frames Analyzed: {request.Landmarks.TotalFramesProcessed}");
        sb.AppendLine();

        // ── Average Joint Angles ──
        sb.AppendLine("=== Average Joint Angles ===");
        sb.AppendLine($"Left Knee: {angles.LeftKneeAngle:F1}° | Right Knee: {angles.RightKneeAngle:F1}°");
        sb.AppendLine($"Left Hip: {angles.LeftHipAngle:F1}° | Right Hip: {angles.RightHipAngle:F1}°");
        sb.AppendLine($"Left Shoulder: {angles.LeftShoulderAngle:F1}° | Right Shoulder: {angles.RightShoulderAngle:F1}°");
        sb.AppendLine($"Left Elbow: {angles.LeftElbowAngle:F1}° | Right Elbow: {angles.RightElbowAngle:F1}°");
        sb.AppendLine($"Trunk Lean: {angles.TrunkLean:F1}°");
        sb.AppendLine();

        // ── Symmetry Analysis ──
        sb.AppendLine("=== Bilateral Symmetry (|Left - Right|) ===");
        sb.AppendLine($"Knee Difference: {Math.Abs(angles.LeftKneeAngle - angles.RightKneeAngle):F1}°");
        sb.AppendLine($"Hip Difference: {Math.Abs(angles.LeftHipAngle - angles.RightHipAngle):F1}°");
        sb.AppendLine($"Shoulder Difference: {Math.Abs(angles.LeftShoulderAngle - angles.RightShoulderAngle):F1}°");
        sb.AppendLine($"Elbow Difference: {Math.Abs(angles.LeftElbowAngle - angles.RightElbowAngle):F1}°");
        sb.AppendLine();

        // ── Per-frame statistics (if we have enough frames) ──
        if (frames is { Count: >= 3 })
        {
            var perFrameAngles = frames
                .Where(f => f.Landmarks is { Count: >= 29 })
                .Select(f => ComputeAnglesFromLandmarks(f.Landmarks))
                .ToList();

            if (perFrameAngles.Count >= 3)
            {
                sb.AppendLine("=== Range of Motion & Consistency (across all frames) ===");
                AppendJointStats(sb, "Left Knee", perFrameAngles.Select(a => a.LeftKnee).ToList());
                AppendJointStats(sb, "Right Knee", perFrameAngles.Select(a => a.RightKnee).ToList());
                AppendJointStats(sb, "Left Hip", perFrameAngles.Select(a => a.LeftHip).ToList());
                AppendJointStats(sb, "Right Hip", perFrameAngles.Select(a => a.RightHip).ToList());
                AppendJointStats(sb, "Left Shoulder", perFrameAngles.Select(a => a.LeftShoulder).ToList());
                AppendJointStats(sb, "Right Shoulder", perFrameAngles.Select(a => a.RightShoulder).ToList());
                AppendJointStats(sb, "Left Elbow", perFrameAngles.Select(a => a.LeftElbow).ToList());
                AppendJointStats(sb, "Right Elbow", perFrameAngles.Select(a => a.RightElbow).ToList());
                AppendJointStats(sb, "Trunk Lean", perFrameAngles.Select(a => a.TrunkLean).ToList());
                sb.AppendLine();

                // ── Temporal Analysis ──
                var third = perFrameAngles.Count / 3;
                var earlyAngles = perFrameAngles.Take(third).ToList();
                var lateAngles = perFrameAngles.Skip(perFrameAngles.Count - third).ToList();

                sb.AppendLine("=== Temporal Analysis (Early vs Late movement) ===");
                AppendTemporalComparison(sb, "Left Knee",
                    earlyAngles.Select(a => a.LeftKnee), lateAngles.Select(a => a.LeftKnee));
                AppendTemporalComparison(sb, "Right Knee",
                    earlyAngles.Select(a => a.RightKnee), lateAngles.Select(a => a.RightKnee));
                AppendTemporalComparison(sb, "Left Hip",
                    earlyAngles.Select(a => a.LeftHip), lateAngles.Select(a => a.LeftHip));
                AppendTemporalComparison(sb, "Right Hip",
                    earlyAngles.Select(a => a.RightHip), lateAngles.Select(a => a.RightHip));
                AppendTemporalComparison(sb, "Trunk Lean",
                    earlyAngles.Select(a => a.TrunkLean), lateAngles.Select(a => a.TrunkLean));
            }
        }

        return sb.ToString();
    }

    // ── Helper: compute joint angles from a single frame's landmarks ──

    private record FrameAngles(
        double LeftKnee, double RightKnee,
        double LeftHip, double RightHip,
        double LeftShoulder, double RightShoulder,
        double LeftElbow, double RightElbow,
        double TrunkLean);

    private static FrameAngles ComputeAnglesFromLandmarks(List<LandmarkPoint> lm)
    {
        // MediaPipe indices: 11=L_shoulder, 12=R_shoulder, 13=L_elbow, 14=R_elbow,
        // 15=L_wrist, 16=R_wrist, 23=L_hip, 24=R_hip,
        // 25=L_knee, 26=R_knee, 27=L_ankle, 28=R_ankle
        return new FrameAngles(
            LeftKnee: CalcAngle(lm[23], lm[25], lm[27]),
            RightKnee: CalcAngle(lm[24], lm[26], lm[28]),
            LeftHip: CalcAngle(lm[11], lm[23], lm[25]),
            RightHip: CalcAngle(lm[12], lm[24], lm[26]),
            LeftShoulder: CalcAngle(lm[23], lm[11], lm[13]),
            RightShoulder: CalcAngle(lm[24], lm[12], lm[14]),
            LeftElbow: CalcAngle(lm[11], lm[13], lm[15]),
            RightElbow: CalcAngle(lm[12], lm[14], lm[16]),
            TrunkLean: CalcTrunkLean(lm[11], lm[12], lm[23], lm[24])
        );
    }

    private static double CalcAngle(LandmarkPoint a, LandmarkPoint b, LandmarkPoint c)
    {
        var radians = Math.Atan2(c.Y - b.Y, c.X - b.X) - Math.Atan2(a.Y - b.Y, a.X - b.X);
        var angle = Math.Abs(radians * 180.0 / Math.PI);
        return angle > 180 ? 360 - angle : angle;
    }

    private static double CalcTrunkLean(LandmarkPoint lShoulder, LandmarkPoint rShoulder,
        LandmarkPoint lHip, LandmarkPoint rHip)
    {
        var midShoulderX = (lShoulder.X + rShoulder.X) / 2;
        var midShoulderY = (lShoulder.Y + rShoulder.Y) / 2;
        var midHipX = (lHip.X + rHip.X) / 2;
        var midHipY = (lHip.Y + rHip.Y) / 2;

        var a = new LandmarkPoint("top", midShoulderX, 0, 0, 1);
        var b = new LandmarkPoint("mid", midShoulderX, midShoulderY, 0, 1);
        var c = new LandmarkPoint("bot", midHipX, midHipY, 0, 1);
        return CalcAngle(a, b, c);
    }

    // ── Helper: append min/max/ROM/StdDev stats for a joint ──

    private static void AppendJointStats(StringBuilder sb, string name, List<double> values)
    {
        var min = values.Min();
        var max = values.Max();
        var avg = values.Average();
        var rom = max - min;
        var stdDev = Math.Sqrt(values.Sum(v => (v - avg) * (v - avg)) / values.Count);

        sb.AppendLine($"{name}: Min={min:F1}° Max={max:F1}° ROM={rom:F1}° StdDev={stdDev:F1}°");
    }

    // ── Helper: compare early vs late averages ──

    private static void AppendTemporalComparison(StringBuilder sb, string name,
        IEnumerable<double> early, IEnumerable<double> late)
    {
        var earlyAvg = early.Average();
        var lateAvg = late.Average();
        var diff = lateAvg - earlyAvg;
        var direction = diff > 0 ? "increased" : "decreased";

        sb.AppendLine($"{name}: Early={earlyAvg:F1}° Late={lateAvg:F1}° Change={Math.Abs(diff):F1}° ({direction})");
    }
}

namespace SportAcademy.Domain.Entities;

public class VideoAnalysis
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string LandmarksJson { get; private set; } = string.Empty;
    public string MovementType { get; private set; } = string.Empty;
    public string AiAnalysisResult { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public AppUser? User { get; set; }

    private VideoAnalysis() { }

    private VideoAnalysis(Guid id, string userId, string movementType,
        string landmarksJson, string aiAnalysisResult)
    {
        Id = id;
        UserId = userId;
        MovementType = movementType;
        LandmarksJson = landmarksJson;
        AiAnalysisResult = aiAnalysisResult;
        CreatedAt = DateTime.UtcNow;
    }

    public static VideoAnalysis Create(Guid id, string userId, string movementType,
        string landmarksJson, string aiAnalysisResult)
        => new(id, userId, movementType, landmarksJson, aiAnalysisResult);
}

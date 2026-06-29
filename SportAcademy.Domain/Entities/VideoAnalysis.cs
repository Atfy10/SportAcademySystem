using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities;

public class VideoAnalysis : ITenantScoped
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// JSON string containing the pose landmarks data from MediaPipe
    /// </summary>
    public string LandmarksJson { get; set; } = string.Empty;

    /// <summary>
    /// The type of movement analyzed (e.g., "running", "walking")
    /// </summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// The AI-generated analysis result (full text)
    /// </summary>
    public string AiAnalysisResult { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation Property
    public AppUser? User { get; set; }
}

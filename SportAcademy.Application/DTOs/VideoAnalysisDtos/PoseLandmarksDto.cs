namespace SportAcademy.Application.DTOs.VideoAnalysisDtos;

/// <summary>
/// Represents a single landmark point from MediaPipe Pose
/// </summary>
public record LandmarkPoint(
    string Name,
    double X,
    double Y,
    double Z,
    double Visibility
);

/// <summary>
/// Represents a single frame's pose landmarks
/// </summary>
public record FrameLandmarks(
    int FrameIndex,
    double TimestampMs,
    List<LandmarkPoint> Landmarks
);

/// <summary>
/// Calculated joint angles from the landmarks
/// </summary>
public record JointAngles(
    double LeftKneeAngle,
    double RightKneeAngle,
    double LeftHipAngle,
    double RightHipAngle,
    double LeftShoulderAngle,
    double RightShoulderAngle,
    double LeftElbowAngle,
    double RightElbowAngle,
    double TrunkLean
);

/// <summary>
/// The complete pose analysis data sent from the frontend
/// </summary>
public record PoseLandmarksDto(
    List<FrameLandmarks> Frames,
    JointAngles AverageAngles,
    int TotalFramesProcessed,
    double VideoDurationSeconds
);

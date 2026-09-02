namespace SportAcademy.Application.DTOs.TraineeDtos;

public record CoachHistoryEntryDto(
    int CoachId,
    string CoachName,
    int SportId,
    string SportName,
    string TraineeGroupName,
    DateTime StartDate,
    DateTime? EndDate,
    int? DurationDays,
    bool IsCurrent,
    int EnrollmentId);

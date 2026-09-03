namespace SportAcademy.Application.DTOs.SessionOccurrenceDtos;

public record SessionOccurrenceDto(
    int Id,
    int TraineeGroupId,
    int GroupScheduleId,
    DateOnly Date,
    string TraineeGroupName,
    string SportName,
    string CoachName,
    string BranchName,
    string StartTime,
    int DurationInMinutes,
    int TotalEnrolled,
    int TotalPresent,
    int TotalLate,
    int TotalAbsent,
    string Status
);

namespace SportAcademy.Application.DTOs.ExcuseRequestDtos;

public record ExcuseRequestDto(
    int Id,
    int SessionOccurrenceId,
    DateOnly SessionDate,
    string SessionStartTime,
    int TraineeId,
    string TraineeName,
    string TraineeGroupName,
    string SportName,
    string Reason,
    string Status,
    DateTime RequestedAt,
    DateTime? ReviewedAt,
    string? ReviewNote
);

namespace SportAcademy.Application.DTOs.AttendanceDtos;

public record AttendanceRecordDto(
    int Id,
    int TraineeId,
    string TraineeName,
    string? CheckInTime,
    string Status
);

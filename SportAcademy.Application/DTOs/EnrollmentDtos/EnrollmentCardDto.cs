namespace SportAcademy.Application.DTOs.EnrollmentDtos;

public record EnrollmentCardDto(
    int Id,
    string TraineeName,
    string? TraineeEmail,
    string Sport,
    string? Program,
    string? Branch,
    string? CoachName,
    string? EnrollmentDate,
    string? StartDate,
    string? EndDate,
    decimal? MonthlyFee,
    string? PaymentStatus,
    string Status,
    int SessionsCompleted,
    int TotalSessions
);

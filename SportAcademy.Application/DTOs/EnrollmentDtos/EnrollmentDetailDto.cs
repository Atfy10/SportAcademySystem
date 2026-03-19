namespace SportAcademy.Application.DTOs.EnrollmentDtos;

public record EnrollmentDetailDto(
    int Id,
    string? TraineeName,
    string? TraineeEmail,
    string? Sport,
    string? Program,
    string? Branch,
    string? CoachName,
    string? EnrollmentDate,
    string? StartDate,
    string? EndDate,
    string? ExpiryDate,
    decimal? MonthlyFee,
    string? PaymentStatus,
    string? Status,
    int? SessionsCompleted,
    int? TotalSessions,
    int? SessionAllowed,
    int? SubscriptionDetailsId
);

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupMemberDto(
    int TraineeId,
    string FullName,
    int Age,
    DateOnly EnrollmentDate,
    string SubscriptionStatus,
    DateOnly EndDate
);

namespace SportAcademy.Application.DTOs.TraineeDtos;

// The reverse of TraineeGroupDropdownDto ("groups eligible for a trainee") - here the group is
// fixed and this lists trainees who already satisfy every check CreateEnrollmentCommandHandler
// would otherwise reject them on: gender policy, sport-specific skill level, an unclaimed active
// subscription for the group's sport, and not already enrolled in another group for that sport.
public record EligibleTraineeForGroupDto(
    int TraineeId,
    string FullName,
    string Gender,
    string SkillLevel,
    int SubscriptionDetailsId,
    DateOnly SubscriptionEndDate);

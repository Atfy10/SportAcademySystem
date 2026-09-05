using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

// GroupType/TrainingDays travel with the subscription so the enrollment step can offer only
// groups that match what it was priced and dated for.
public record SubscriptionDetailsDropdownDto(
    int Id,
    string Name,
    int SportId,
    DateOnly EndDate,
    TraineeGroupType GroupType,
    /// <summary>Day-of-week names ("Sunday"), as elsewhere in this API - see GroupDayPatternDto.</summary>
    List<string> TrainingDays,
    /// <summary>
    /// Sessions the resulting enrollment will be granted. This is the subscription's own figure
    /// (the plan's DaysPerMonth, via SubscriptionDetailsService.CalculateAllowedSessions) - the
    /// exact value CreateEnrollmentCommandHandler assigns, so the form can show what will
    /// actually be stored instead of estimating it.
    /// </summary>
    int SessionsAllowed);

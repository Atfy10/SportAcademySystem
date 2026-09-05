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
    List<DayOfWeek> TrainingDays);

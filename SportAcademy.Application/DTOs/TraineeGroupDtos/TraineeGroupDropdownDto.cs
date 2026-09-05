using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupDropdownDto(
    int Id,
    string Name,
    int SportId,
    string BranchName,
    string CoachName,
    SkillLevel SkillLevel,
    TraineeGroupGender Gender,
    TraineeGroupType Type,
    /// <summary>Day-of-week names ("Sunday"), as elsewhere in this API - see GroupDayPatternDto.</summary>
    List<string> TrainingDays);

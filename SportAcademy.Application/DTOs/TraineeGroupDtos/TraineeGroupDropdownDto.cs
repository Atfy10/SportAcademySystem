using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupDropdownDto(
    int Id,
    string Name,
    int SportId,
    string BranchName,
    string CoachName,
    SkillLevel SkillLevel,
    TraineeGroupGender Gender);

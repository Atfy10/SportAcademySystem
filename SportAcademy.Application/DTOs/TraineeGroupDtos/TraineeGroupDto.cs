using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupDto(
    int Id,
    SkillLevel SkillLevel,
    int MaximumCapacity,
    int DurationInMinutes,
    Gender Gender,
    int BranchId,
    int CoachId
);

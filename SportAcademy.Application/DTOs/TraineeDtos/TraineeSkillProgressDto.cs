using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public record SkillLevelPeriodDto(SkillLevel SkillLevel, DateTime StartDate, DateTime? EndDate, int? DurationDays);

public record TraineeSportSkillProgressDto(
    int SportId, string SportName, SkillLevel CurrentSkillLevel, List<SkillLevelPeriodDto> History);

using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupCardDto
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public string SportName { get; init; } = null!;
    public string CoachName { get; init; } = null!;
    public string BranchName { get; init; } = null!;
    public int DurationInMinutes { get; init; }
    public int TraineesCount { get; init; }
    public IReadOnlyList<GroupSchedulesTimesDto> Schedules { get; init; } = [];
}

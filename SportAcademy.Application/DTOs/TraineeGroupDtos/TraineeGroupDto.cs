using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos
{
    public record TraineeGroupDto(
        int Id,
        SkillLevel SkillLevel,
        int MaximumCapacity,
        int DurationInMinutes,
        Gender Gender,
        int BranchId,
        int CoachId
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.CoachDtos
{
    public record CoachSummaryDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string SportName { get; init; } = null!;
        public DateOnly BirthDate { get; init; }
        public string SkillLevel { get; init; } = null!;
    }
}

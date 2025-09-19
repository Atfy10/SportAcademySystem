using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Extensions
{
    public static class EntityTypeEnumExtentions
    {
        public static string DisplayName(this EntityTypes entityType)
            => entityType switch
            {
                EntityTypes.Trainee => "Trainee",
                EntityTypes.Branch => "Branch",
                EntityTypes.User => "User",
                EntityTypes.Enrollment => "Enrollment",
                EntityTypes.Sport => "Sport",
                EntityTypes.Attendance => "Attendance",
                EntityTypes.Coach => "Coach",
                EntityTypes.Employee => "Employee",
                EntityTypes.Session => "Session",
                EntityTypes.Payment => "Payment",
                _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
            };
    }
}

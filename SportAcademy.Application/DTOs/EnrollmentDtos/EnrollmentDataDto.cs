using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.EnrollmentDtos;

public record EnrollmentDataDto(
    int Id,
    DateTime EnrollmentDate,
    DateTime ExpiryDate,
    int SessionAllowed,
    int SessionRemaining,
    bool IsActive,
    string TraineeName,
    string TraineeGroupCoachName,
    int SubscriptionDetailsId
);

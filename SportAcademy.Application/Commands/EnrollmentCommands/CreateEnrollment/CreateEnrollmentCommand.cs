using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment
{
    public record CreateEnrollmentCommand(
        DateTime EnrollmentDate,
        DateTime ExpiryDate,
        int? SessionAllowed,
        int TraineeId,
        int TraineeGroupId,
        int SubscriptionDetailsId
    ) : IRequest<Result<int>>;
}

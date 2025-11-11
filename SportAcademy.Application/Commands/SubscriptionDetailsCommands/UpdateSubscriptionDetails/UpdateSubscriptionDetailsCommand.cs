using MediatR;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails
{
    public record UpdateSubscriptionDetailsCommand(
        int Id,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string? PaymentNumber,
        int? TraineeId,
        int? SubscriptionTypeId,
        int? SportId,
        int? BranchId
        ) : IRequest<Result<SubscriptionDetailsDto>>;
}

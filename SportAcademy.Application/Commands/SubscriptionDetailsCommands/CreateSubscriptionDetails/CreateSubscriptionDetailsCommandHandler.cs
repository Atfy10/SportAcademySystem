using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    // No discount field on this command, deliberately - a discount code always goes through
    // SubscriptionDiscountRequestCommands instead (create -> Owner/Admin/Accountant approve ->
    // ISubscriptionCreationService.CreateAsync). This handler stays the fast, immediate,
    // no-approval path for the common case.
    public class CreateSubscriptionDetailsCommandHandler
        : IRequestHandler<CreateSubscriptionDetailsCommand, Result<SubscriptionCreatedDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionCreationService _subscriptionCreationService;
        private readonly IUserContextService _userContext;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionCreationService subscriptionCreationService,
            IUserContextService userContext)
        {
            _subscriptionCreationService = subscriptionCreationService;
            _userContext = userContext;
        }

        public async Task<Result<SubscriptionCreatedDto>> Handle(
            CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var created = await _subscriptionCreationService.CreateAsync(
                new SubscriptionCreationRequest(
                    request.TraineeId, request.SubscriptionTypeId, request.SportId, request.BranchId,
                    request.StartDate, request.GroupType, request.TrainingDays, request.PaymentTypeId,
                    DiscountPercentage: null, DiscountCodeId: null,
                    ActingUserId: _userContext.UserId,
                    DepositAmount: request.PayDeposit ? request.DepositAmount : null,
                    BalanceDueDate: request.PayDeposit ? request.BalanceDueDate : null),
                ct: cancellationToken);

            return Result<SubscriptionCreatedDto>.Success(SubscriptionCreatedDto.From(created), _operation);
        }
    }
}

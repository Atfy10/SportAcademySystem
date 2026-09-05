using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    // No discount field on this command, deliberately - a discount code always goes through
    // SubscriptionDiscountRequestCommands instead (create -> Owner/Admin/Accountant approve ->
    // ISubscriptionCreationService.CreateAsync). This handler stays the fast, immediate,
    // no-approval path for the common case; discountAmount/discountCodeId are always 0/null here.
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
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

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionCreationService.CreateAsync(
                request.TraineeId, request.SubscriptionTypeId, request.SportId, request.BranchId,
                request.StartDate, request.GroupType, request.TrainingDays, request.PaymentTypeId,
                discountPercentage: null, discountCodeId: null,
                actingUserId: _userContext.UserId,
                cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}

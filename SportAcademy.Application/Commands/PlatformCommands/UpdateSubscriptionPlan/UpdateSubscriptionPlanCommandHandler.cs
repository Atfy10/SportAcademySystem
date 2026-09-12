using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler
    : IRequestHandler<UpdateSubscriptionPlanCommand, Result<SubscriptionPlanSummaryDto>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateSubscriptionPlanCommandHandler(
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionPlanSummaryDto>> Handle(
        UpdateSubscriptionPlanCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result<SubscriptionPlanSummaryDto>.Failure(_operation, "Subscription plan not found.", 404);

        request.ResolvedBeforeState = new
        {
            plan.Name,
            plan.Description,
            plan.MonthlyPrice,
            plan.YearlyPrice,
            plan.IsPubliclyListed,
            plan.DisplayOrder,
            plan.IsHighlighted
        };

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.YearlyPrice = request.YearlyPrice;
        plan.IsPubliclyListed = request.IsPubliclyListed;
        plan.DisplayOrder = request.DisplayOrder;
        plan.IsHighlighted = request.IsHighlighted;

        await _planRepository.UpdateAsync(plan, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new SubscriptionPlanSummaryDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Code = plan.Code,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice,
            YearlyPrice = plan.YearlyPrice,
            IsPubliclyListed = plan.IsPubliclyListed,
            DisplayOrder = plan.DisplayOrder,
            IsHighlighted = plan.IsHighlighted,
        };

        return Result<SubscriptionPlanSummaryDto>.Success(dto, _operation, $"Updated {plan.Name} plan.");
    }
}

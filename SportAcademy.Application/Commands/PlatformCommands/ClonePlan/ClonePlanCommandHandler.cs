using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ClonePlan;

public class ClonePlanCommandHandler : IRequestHandler<ClonePlanCommand, Result<SubscriptionPlanSummaryDto>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Add.ToString();

    public ClonePlanCommandHandler(
        IBaseRepository<SubscriptionPlan, int> planRepository,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionPlanSummaryDto>> Handle(ClonePlanCommand request, CancellationToken ct)
    {
        var source = await _planRepository.GetByIdAsync(request.SourcePlanId, ct);
        if (source is null)
            return Result<SubscriptionPlanSummaryDto>.Failure(_operation, "Source plan not found.", 404);

        var allPlans = await _planRepository.GetAllAsync(ct);
        if (allPlans.Any(p => p.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            return Result<SubscriptionPlanSummaryDto>.Failure(_operation, "A plan with this code already exists.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(request.OwnerTenantId, ct);
        if (tenant is null)
            return Result<SubscriptionPlanSummaryDto>.Failure(_operation, "Tenant not found.", 404);

        var newPlan = new SubscriptionPlan
        {
            Name = request.Name,
            Code = request.Code,
            Description = source.Description,
            MonthlyPrice = source.MonthlyPrice,
            YearlyPrice = source.YearlyPrice,
            IsActive = true,
            // Never publicly listed, regardless of what the source plan was - see
            // GetPublicPlansQueryHandler's belt-and-braces !IsCustom filter for the second line
            // of defense against this ever leaking to the marketing site anyway.
            IsPubliclyListed = false,
            IsHighlighted = false,
            DisplayOrder = 0,
            IsCustom = true,
            OwnerTenantId = request.OwnerTenantId,
        };

        // AddAsync saves immediately (see BaseRepository), so newPlan.Id is populated by the
        // time the feature/limit copies below need it.
        await _planRepository.AddAsync(newPlan, ct);

        var sourceFeatureIds = await _tenantRepository.GetPlanFeaturesAsync(source.Id, ct);
        await _tenantRepository.ReplacePlanFeaturesAsync(newPlan.Id, sourceFeatureIds, ct);

        var sourceLimits = await _tenantRepository.GetPlanLimitsAsync(source.Id, ct);
        await _tenantRepository.ReplacePlanLimitsAsync(
            newPlan.Id, sourceLimits.ToDictionary(l => l.ResourceKey, l => l.MaxCount), ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new SubscriptionPlanSummaryDto
        {
            Id = newPlan.Id,
            Name = newPlan.Name,
            Code = newPlan.Code,
            Description = newPlan.Description,
            MonthlyPrice = newPlan.MonthlyPrice,
            YearlyPrice = newPlan.YearlyPrice,
            IsPubliclyListed = newPlan.IsPubliclyListed,
            DisplayOrder = newPlan.DisplayOrder,
            IsHighlighted = newPlan.IsHighlighted,
        };

        return Result<SubscriptionPlanSummaryDto>.Success(
            dto, _operation, $"Cloned '{source.Name}' into a new custom plan '{newPlan.Name}' for {tenant.DisplayName}.");
    }
}

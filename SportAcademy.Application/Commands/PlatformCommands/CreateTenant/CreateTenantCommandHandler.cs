using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDetailResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly string _operation = OperationType.Add.ToString();

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<TenantDetailResponse>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var slugUnique = await _tenantRepository.IsSlugUniqueAsync(request.Slug, null, ct);
        if (!slugUnique)
            return Result<TenantDetailResponse>.Failure(_operation, "Slug is already in use.", 400);

        var codeUnique = await _tenantRepository.IsCodeUniqueAsync(request.Code, null, ct);
        if (!codeUnique)
            return Result<TenantDetailResponse>.Failure(_operation, "Code is already in use.", 400);

        var plan = await _planRepository.GetByIdAsync(request.SubscriptionPlanId, ct);
        if (plan is null)
            return Result<TenantDetailResponse>.Failure(_operation, "Subscription plan not found.", 404);

        var now = DateTime.UtcNow;

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DisplayName = request.DisplayName,
            Slug = request.Slug,
            Code = request.Code.ToUpper(),
            Email = request.Email,
            Status = TenantStatus.PendingSetup,
            CreatedAt = now,
            Profile = new TenantProfile
            {
                OrganizationName = request.DisplayName,
                Phone = request.Phone,
                Address = request.Address
            },
            Settings = new TenantSettings
            {
                TimeZone = request.TimeZone ?? "UTC",
                Language = request.Language ?? "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "HH:mm",
                Currency = request.Currency ?? "USD"
            },
            Subscription = new TenantSubscription
            {
                StartsAt = now,
                EndsAt = now.AddYears(1),
                IsTrial = true,
                AutoRenew = true,
                SubscriptionPlanId = plan.Id
            }
        };

        await _tenantRepository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _mediator.Publish(
            new TenantCreatedEvent(tenant.Id, tenant.Slug, request.OwnerEmail, request.OwnerName), ct);

        var detail = tenant.ToDetailResponse();
        return Result<TenantDetailResponse>.Success(detail, _operation, "Tenant created successfully.");
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantFeatures;

public class GetTenantFeaturesQueryHandler : IRequestHandler<GetTenantFeaturesQuery, Result<List<TenantFeatureResponse>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantFeaturesQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<TenantFeatureResponse>>> Handle(GetTenantFeaturesQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<List<TenantFeatureResponse>>.Failure(_operation, "Tenant not found.", 404);

        var features = await _tenantRepository.GetAllFeaturesAsync(ct);
        var tenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(request.TenantId, ct);

        var response = features.Select(f =>
        {
            var tf = tenantFeatures.FirstOrDefault(x => x.FeatureId == f.Id);
            return new TenantFeatureResponse
            {
                FeatureId = f.Id,
                Name = f.Name,
                DisplayName = f.DisplayName,
                Description = f.Description,
                IsEnabled = tf?.IsEnabled ?? false,
                EnabledAt = tf?.EnabledAt
            };
        }).ToList();

        return Result<List<TenantFeatureResponse>>.Success(response, _operation);
    }
}

using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateFeatureBundlePricing;

public class UpdateFeatureBundlePricingCommandHandler
    : IRequestHandler<UpdateFeatureBundlePricingCommand, Result<BundleFeatureDto>>
{
    private readonly IBaseRepository<Feature, Guid> _featureRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateFeatureBundlePricingCommandHandler(
        IBaseRepository<Feature, Guid> featureRepository, IUnitOfWork unitOfWork)
    {
        _featureRepository = featureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BundleFeatureDto>> Handle(UpdateFeatureBundlePricingCommand request, CancellationToken ct)
    {
        var feature = await _featureRepository.GetByIdAsync(request.FeatureId, ct);
        if (feature is null)
            return Result<BundleFeatureDto>.Failure(_operation, "Feature not found.", 404);

        if (!feature.IsImplemented)
            return Result<BundleFeatureDto>.Failure(_operation, "Can't price a feature that isn't implemented yet.", 400);

        request.ResolvedBeforeState = new { feature.BundlePrice, feature.IsBundleCore };

        feature.BundlePrice = request.BundlePrice;
        feature.IsBundleCore = request.IsCore;

        await _featureRepository.UpdateAsync(feature, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new BundleFeatureDto
        {
            Id = feature.Id,
            Name = feature.Name,
            DisplayName = feature.DisplayName,
            Description = feature.Description,
            Category = FeatureCategories.For(feature.Name),
            BundlePrice = feature.BundlePrice,
            IsCore = feature.IsBundleCore,
            DirectPrerequisites = FeatureDependencies.GetPrerequisites(feature.Name).ToList(),
            DirectDependents = FeatureDependencies.GetDependents(feature.Name).ToList(),
        };

        return Result<BundleFeatureDto>.Success(dto, _operation, $"Updated pricing for {feature.DisplayName}.");
    }
}

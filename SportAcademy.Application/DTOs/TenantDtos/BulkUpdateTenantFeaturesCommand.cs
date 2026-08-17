using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record BulkUpdateTenantFeaturesCommand(Dictionary<Guid, bool> FeatureStates) : IRequest<Result>;

using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record UpdateTenantFeatureCommand(Guid FeatureId, bool IsEnabled) : IRequest<Result>;

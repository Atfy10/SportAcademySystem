using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;

public record ToggleFeatureCommand(Guid TenantId, Guid FeatureId, bool IsEnabled) : IRequest<Result>;

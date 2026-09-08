using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.SendInvitationEmail;

// TenantId is not strictly needed to find the invitation (RawToken alone resolves it uniquely),
// but the caller always reaches this through a tenant-scoped route/authorization check (see
// OnboardingController) - carrying it lets the handler confirm the token it was handed actually
// belongs to the tenant the caller was authorized against, rather than trusting the route alone.
public record SendInvitationEmailCommand(Guid TenantId, string RawToken) : IRequest<Result>;

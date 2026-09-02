using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ProfileCommands.CompleteOnboarding
{
    public record CompleteOnboardingCommand(string? PreferredLanguage) : IRequest<Result<bool>>;
}

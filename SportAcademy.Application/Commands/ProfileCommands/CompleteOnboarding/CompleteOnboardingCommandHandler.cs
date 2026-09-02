using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.ProfileCommands.CompleteOnboarding
{
    public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result<bool>>
    {
        private static readonly string[] SupportedLanguages = ["en", "ar"];

        private readonly IProfileRepository _profileRepository;
        private readonly IUserContextService _userContext;
        private readonly string _operation = OperationType.Update.ToString();

        public CompleteOnboardingCommandHandler(
            IProfileRepository profileRepository,
            IUserContextService userContext)
        {
            _profileRepository = profileRepository;
            _userContext = userContext;
        }

        public async Task<Result<bool>> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            if (userId is null)
                return Result<bool>.Failure(_operation, "User ID is not available in the context.", 400);

            // Every AppUser is meant to get a companion Profile row at creation time (see
            // AcceptInvitationCommandHandler / AppDataSeeder), but this table predates that
            // guarantee being consistently enforced across every creation path - rather than
            // 404 a user out of finishing onboarding over a row that should exist, create it
            // on the spot.
            var profile = await _profileRepository.GetByAppUserIdAsync(userId.Value, cancellationToken);
            var isNew = profile is null;
            profile ??= new Profile { AppUserId = userId.Value };

            profile.HasCompletedOnboarding = true;

            // Silently ignored rather than rejected - an unrecognized/missing language just
            // means "don't update the stored preference", not a client error worth a 400.
            if (request.PreferredLanguage != null && SupportedLanguages.Contains(request.PreferredLanguage))
                profile.PreferredLanguage = request.PreferredLanguage;

            if (isNew)
            {
                try
                {
                    await _profileRepository.AddAsync(profile, cancellationToken);
                }
                catch (DbUpdateException)
                {
                    // AppUserId is Profile's PK - a concurrent request for the same user (e.g.
                    // a double-submitted Finish/Skip) can race both requests past the null check
                    // above and both attempt an insert; the loser hits a PK violation here. The
                    // row now exists either way, so finish this request as an update instead of
                    // surfacing a 500 for something that already succeeded.
                    var existing = await _profileRepository.GetByAppUserIdAsync(userId.Value, cancellationToken)
                        ?? throw new InvalidOperationException("Profile insert conflicted but no row was found on retry.");
                    existing.HasCompletedOnboarding = true;
                    if (request.PreferredLanguage != null && SupportedLanguages.Contains(request.PreferredLanguage))
                        existing.PreferredLanguage = request.PreferredLanguage;
                    await _profileRepository.UpdateAsync(existing, cancellationToken);
                }
            }
            else
            {
                await _profileRepository.UpdateAsync(profile, cancellationToken);
            }

            return Result<bool>.Success(true, _operation);
        }
    }
}

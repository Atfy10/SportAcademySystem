using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetPublicEventTicket
{
    public class GetPublicEventTicketQueryHandler : IRequestHandler<GetPublicEventTicketQuery, Result<PublicEventTicketDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly IEventTicketStore _store;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly ITenantClock _tenantClock;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetPublicEventTicketQueryHandler(
            IEventTicketStore store,
            ITenantRepository tenantRepository,
            ITenantIdProvider tenantIdProvider,
            ITenantClock tenantClock,
            ICurrentLanguageProvider languageProvider)
        {
            _store = store;
            _tenantRepository = tenantRepository;
            _tenantIdProvider = tenantIdProvider;
            _tenantClock = tenantClock;
            _languageProvider = languageProvider;
        }

        public async Task<Result<PublicEventTicketDto>> Handle(GetPublicEventTicketQuery request, CancellationToken ct)
        {
            var token = EventTicketCheckService.ParseToken(request.Token);
            var t = token is null ? null : await _store.FindPublicAsync(token, _languageProvider.Language, ct);

            // An unknown, revoked or re-issued code, an academy that isn't open for business, or
            // one whose plan no longer includes events: all just "invalid" - nothing leaks.
            if (t is null
                || t.TenantStatus != TenantStatus.Active
                || !await _tenantRepository.IsFeatureEnabledAsync(t.TenantId, "event-management", ct))
            {
                return Success(new PublicEventTicketDto(EventTicketCheckResult.Invalid));
            }

            // No signed-in user: read the academy's time zone inside its own tenant.
            using var _ = _tenantIdProvider.Impersonate(t.TenantId);
            TenantCalendar.SetTimeZone(await _tenantClock.GetTimeZoneAsync(ct));

            var now = DateTime.UtcNow;

            // Terminated for good: the ticket says so, and nothing else - no code, no ticket details.
            if (EventEntryRules.TicketsTerminated(t.IsCancelled, t.EndsAtUtc, now))
            {
                return Success(new PublicEventTicketDto(
                    t.IsCancelled ? EventTicketCheckResult.Cancelled : EventTicketCheckResult.Ended,
                    t.AcademyName,
                    t.EventTitle));
            }

            var result = t.AdmittedAt is not null
                ? EventTicketCheckResult.AlreadyUsed
                : EventEntryRules.Closed(t.IsCancelled, t.StartsAtUtc, t.EndsAtUtc, now) ?? EventTicketCheckResult.Valid;

            return Success(new PublicEventTicketDto(
                result,
                t.AcademyName,
                t.EventTitle,
                t.BranchName,
                TenantCalendar.ToLocal(t.StartsAtUtc),
                TenantCalendar.ToLocal(t.EndsAtUtc),
                TenantCalendar.ToLocal(EventEntryRules.OpensAt(t.StartsAtUtc)),
                t.Number,
                t.GuestName,
                t.AdmittedAt is { } at ? TenantCalendar.ToLocal(at) : null));
        }

        private Result<PublicEventTicketDto> Success(PublicEventTicketDto dto) => Result<PublicEventTicketDto>.Success(dto, _operation);
    }
}

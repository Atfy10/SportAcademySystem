using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventCommands.ScanEventEntry
{
    public class ScanEventEntryCommandHandler : IRequestHandler<ScanEventEntryCommand, Result<EventEntryResultDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly IEventEntryStore _store;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly ITenantClock _tenantClock;
        private readonly ICurrentLanguageProvider _languageProvider;

        public ScanEventEntryCommandHandler(
            IEventEntryStore store,
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

        public async Task<Result<EventEntryResultDto>> Handle(ScanEventEntryCommand request, CancellationToken ct)
        {
            var target = await _store.FindByTokenAsync(request.Token.Trim().ToLowerInvariant(), _languageProvider.Language, ct);

            // An unknown or replaced code, an academy that isn't open for business, or one whose
            // plan no longer includes events: all just "invalid" - nothing about the event leaks.
            if (target is null
                || target.TenantStatus != TenantStatus.Active
                || !await _tenantRepository.IsFeatureEnabledAsync(target.TenantId, "event-management", ct))
            {
                return Success(new EventEntryResultDto(EventEntryResult.Invalid));
            }

            // The scan has no signed-in user, so act inside the event's own academy explicitly:
            // its query filters, its time zone, and the tenant write guard all key off this.
            using var _ = _tenantIdProvider.Impersonate(target.TenantId);
            TenantCalendar.SetTimeZone(await _tenantClock.GetTimeZoneAsync(ct));

            var closed = EventEntryRules.Closed(target.IsCancelled, target.StartsAtUtc, target.EndsAtUtc, DateTime.UtcNow);
            if (closed is { } reason)
                return Success(Describe(target, reason, target.AdmittedCount, null));

            var previous = await _store.FindAdmissionAsync(target.EventId, request.DeviceKey, ct);
            if (previous is not null)
                return Success(Describe(target, EventEntryResult.AlreadyAdmitted, target.AdmittedCount, previous));

            var outcome = await _store.TryAdmitAsync(target.EventId, request.DeviceKey, ct);
            return Success(Describe(target, outcome.Result, outcome.AdmittedCount, outcome.Admission));
        }

        private Result<EventEntryResultDto> Success(EventEntryResultDto dto) => Result<EventEntryResultDto>.Success(dto, _operation);

        private static EventEntryResultDto Describe(
            EventEntryTarget t, EventEntryResult result, int admittedCount, EventAdmission? admission)
            => new(
                result,
                t.Title,
                t.AcademyName,
                t.BranchName,
                TenantCalendar.ToLocal(t.StartsAtUtc),
                TenantCalendar.ToLocal(t.EndsAtUtc),
                TenantCalendar.ToLocal(EventEntryRules.OpensAt(t.StartsAtUtc)),
                t.Capacity,
                admittedCount,
                admission?.Number,
                admission is null ? null : TenantCalendar.ToLocal(admission.AdmittedAt));
    }
}

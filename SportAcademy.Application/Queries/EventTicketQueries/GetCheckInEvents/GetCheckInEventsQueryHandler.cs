using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetCheckInEvents
{
    public class GetCheckInEventsQueryHandler : IRequestHandler<GetCheckInEventsQuery, Result<List<CheckInEventDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IEventTicketStore _store;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetCheckInEventsQueryHandler(IEventTicketStore store, ICurrentLanguageProvider languageProvider)
        {
            _store = store;
            _languageProvider = languageProvider;
        }

        public async Task<Result<List<CheckInEventDto>>> Handle(GetCheckInEventsQuery request, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var endOfToday = TenantCalendar.DayStartUtc(TenantCalendar.Today.AddDays(1));
            var rows = await _store.GetCheckInEventsAsync(now, endOfToday, _languageProvider.Language, ct);

            var events = rows.Select(r => new CheckInEventDto(
                    r.Id,
                    r.Title,
                    r.BranchName,
                    TenantCalendar.ToLocal(r.StartsAtUtc),
                    TenantCalendar.ToLocal(r.EndsAtUtc),
                    TenantCalendar.ToLocal(EventEntryRules.OpensAt(r.StartsAtUtc)),
                    r.Capacity,
                    r.Issued,
                    r.Admitted,
                    EventEntryRules.Closed(false, r.StartsAtUtc, r.EndsAtUtc, now) is null))
                .ToList();

            return Result<List<CheckInEventDto>>.Success(events, _operation);
        }
    }
}

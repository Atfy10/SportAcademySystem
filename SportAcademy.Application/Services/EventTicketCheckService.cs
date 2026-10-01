using System.Text.RegularExpressions;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Services
{
    // The door scanner's shared logic: find the scanned ticket (by its QR code, or by event +
    // ticket number when the code can't be scanned), judge it, and describe it for the screen.
    // Runs inside the doorman's own tenant and branch scope - a ticket for another academy or a
    // branch they don't cover is simply "invalid".
    public partial class EventTicketCheckService
    {
        private readonly IEventTicketStore _store;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentLanguageProvider _languageProvider;

        public EventTicketCheckService(
            IEventTicketStore store,
            IUserRepository userRepository,
            ICurrentLanguageProvider languageProvider)
        {
            _store = store;
            _userRepository = userRepository;
            _languageProvider = languageProvider;
        }

        // A ticket's QR code holds its link (".../ticket/{token}"); staff may also type or paste
        // just the token. Anything that isn't a 32-hex token is no ticket at all.
        public static string? ParseToken(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var text = code.Trim();
            var cut = text.IndexOfAny(['?', '#']);
            if (cut >= 0) text = text[..cut];
            var token = text.TrimEnd('/').Split('/')[^1].ToLowerInvariant();
            return TokenPattern().IsMatch(token) ? token : null;
        }

        public async Task<EventTicket?> FindAsync(string? code, int? eventId, int? number, CancellationToken ct)
        {
            if (eventId is { } id && number is { } n)
                return await _store.FindByNumberAsync(id, n, ct);

            return ParseToken(code) is { } token ? await _store.FindByTokenAsync(token, ct) : null;
        }

        // Why this ticket can't be let in now (Ended/Cancelled/NotYetOpen/AlreadyUsed), or Valid.
        public static EventTicketCheckResult Judge(EventTicket ticket, DateTime nowUtc)
        {
            var ev = ticket.Event;
            if (EventEntryRules.Closed(ev.IsCancelled, ev.StartsAt, ev.EndsAt, nowUtc) is { } closed)
                return closed;
            return ticket.AdmittedAt is null ? EventTicketCheckResult.Valid : EventTicketCheckResult.AlreadyUsed;
        }

        public async Task<EventTicketCheckDto> DescribeAsync(EventTicket ticket, EventTicketCheckResult result, CancellationToken ct)
        {
            var ev = ticket.Event;
            var lang = _languageProvider.Language;
            var counts = await _store.GetCountsAsync(ev.Id, ct);

            string? admittedBy = null;
            if (ticket.AdmittedByUserId is { } by)
                admittedBy = (await _userRepository.GetDisplayNamesAsync([by], ct)).GetValueOrDefault(by);

            return new EventTicketCheckDto(
                result,
                ticket.Id,
                ticket.Number,
                ticket.GuestName,
                ev.Id,
                ev.Title,
                ev.Branch?.Translations.FirstOrDefault(t => t.LangCode == lang)?.Name ?? ev.Branch?.Name,
                TenantCalendar.ToLocal(ev.StartsAt),
                TenantCalendar.ToLocal(ev.EndsAt),
                TenantCalendar.ToLocal(EventEntryRules.OpensAt(ev.StartsAt)),
                ev.Capacity,
                counts.Admitted,
                ticket.AdmittedAt is { } at ? TenantCalendar.ToLocal(at) : null,
                admittedBy);
        }

        [GeneratedRegex("^[0-9a-f]{32}$")]
        private static partial Regex TokenPattern();
    }
}

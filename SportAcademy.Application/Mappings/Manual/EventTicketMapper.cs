using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Mappings.Manual
{
    public static class EventTicketMapper
    {
        public static EventTicketDto ToDto(EventTicket t, IReadOnlyDictionary<Guid, string> userNames)
            => new(
                t.Id,
                t.Number,
                t.Token,
                t.GuestName,
                DateTime.SpecifyKind(t.IssuedAt, DateTimeKind.Utc),
                t.AdmittedAt is { } at ? TenantCalendar.ToLocal(at) : null,
                t.AdmittedByUserId is { } by ? userNames.GetValueOrDefault(by) : null);

        public static IEnumerable<Guid> UserIds(IEnumerable<EventTicket> tickets)
            => tickets.Where(t => t.AdmittedByUserId is not null).Select(t => t.AdmittedByUserId!.Value).Distinct();

        // Names are optional and trimmed; blank means no name.
        public static string? CleanName(string? name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }
}

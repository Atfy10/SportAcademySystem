using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Services
{
    // Loads one event as the details page (and every event command's response) shows it: the
    // event, its bill, and each payment applied to that bill. Shared so Create/Update/Cancel
    // return exactly what GetEventById would.
    public class EventDetailsLoader
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentLanguageProvider _languageProvider;

        public EventDetailsLoader(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            ICurrentLanguageProvider languageProvider)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _languageProvider = languageProvider;
        }

        public async Task<EventDetailsDto> LoadAsync(int id, CancellationToken ct)
        {
            var ev = await _eventRepository.GetWithDetailsAsync(id, forUpdate: false, ct)
                ?? throw new EventNotFoundException(id.ToString());

            var lang = _languageProvider.Language;
            var names = await _userRepository.GetDisplayNamesAsync(EventMapper.UserIds([ev]), ct);
            var dto = EventMapper.ToDto(ev, lang, TenantCalendar.Today, names);

            var payments = (ev.Invoice?.Allocations ?? [])
                .OrderBy(a => a.Payment.PaidDate)
                .Select(a => new EventPaymentDto(
                    a.PaymentNumber,
                    a.Payment.PaidDate,
                    a.Amount,
                    a.ReversedAmount,
                    a.Payment.PaymentType.Translations.FirstOrDefault(t => t.LangCode == lang)?.Name
                        ?? a.Payment.PaymentType.Name,
                    a.Payment.Status))
                .ToList();

            return new EventDetailsDto(dto, payments);
        }
    }
}

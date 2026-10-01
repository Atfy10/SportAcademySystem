using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventCommands.RegenerateEventEntryCode
{
    public class RegenerateEventEntryCodeCommandHandler : IRequestHandler<RegenerateEventEntryCodeCommand, Result<EventDetailsDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly EventDetailsLoader _detailsLoader;

        public RegenerateEventEntryCodeCommandHandler(IEventRepository eventRepository, EventDetailsLoader detailsLoader)
        {
            _eventRepository = eventRepository;
            _detailsLoader = detailsLoader;
        }

        public async Task<Result<EventDetailsDto>> Handle(RegenerateEventEntryCodeCommand request, CancellationToken ct)
        {
            var ev = await _eventRepository.GetByIdAsync(request.Id, ct)
                ?? throw new EventNotFoundException(request.Id.ToString());

            if (ev.IsCancelled)
                throw EventRuleException.CancelledReadOnly();

            ev.EntryToken = EventEntryRules.NewToken();
            await _eventRepository.UpdateAsync(ev, ct);

            return Result<EventDetailsDto>.Success(await _detailsLoader.LoadAsync(ev.Id, ct), _operation);
        }
    }
}

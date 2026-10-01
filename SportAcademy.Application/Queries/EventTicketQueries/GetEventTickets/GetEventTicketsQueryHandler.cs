using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetEventTickets
{
    public class GetEventTicketsQueryHandler : IRequestHandler<GetEventTicketsQuery, Result<EventTicketsDto>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IEventTicketStore _store;
        private readonly IUserRepository _userRepository;

        public GetEventTicketsQueryHandler(
            IEventRepository eventRepository, IEventTicketStore store, IUserRepository userRepository)
        {
            _eventRepository = eventRepository;
            _store = store;
            _userRepository = userRepository;
        }

        public async Task<Result<EventTicketsDto>> Handle(GetEventTicketsQuery request, CancellationToken ct)
        {
            var ev = await _eventRepository.GetByIdAsync(request.EventId, ct)
                ?? throw new EventNotFoundException(request.EventId.ToString());

            var counts = await _store.GetCountsAsync(ev.Id, ct);
            var (items, totalCount) = await _store.GetPagedAsync(ev.Id, request.Filter, request.Term, request.Page, ct);
            var names = await _userRepository.GetDisplayNamesAsync(EventTicketMapper.UserIds(items), ct);

            return Result<EventTicketsDto>.Success(new EventTicketsDto(
                ev.Id,
                ev.Capacity,
                counts.Issued,
                counts.Admitted,
                EventEntryRules.TicketsTerminated(ev.IsCancelled, ev.EndsAt, DateTime.UtcNow),
                TenantCalendar.ToLocal(EventEntryRules.OpensAt(ev.StartsAt)),
                new PagedData<EventTicketDto>
                {
                    Items = items.Select(t => EventTicketMapper.ToDto(t, names)).ToList(),
                    TotalCount = totalCount,
                    Page = request.Page.Page,
                    PageSize = request.Page.PageSize,
                }), _operation);
        }
    }
}

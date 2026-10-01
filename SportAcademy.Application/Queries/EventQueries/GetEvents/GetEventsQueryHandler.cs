using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventQueries.GetEvents;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<PagedData<EventDto>>>
{
    private readonly IEventRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetEventsQueryHandler(
        IEventRepository repository,
        IUserRepository userRepository,
        ICurrentLanguageProvider languageProvider)
    {
        _repository = repository;
        _userRepository = userRepository;
        _languageProvider = languageProvider;
    }

    public async Task<Result<PagedData<EventDto>>> Handle(GetEventsQuery request, CancellationToken ct)
    {
        var today = TenantCalendar.Today;
        var filter = new EventListFilter(
            request.BranchId, request.From, request.To, request.Status,
            request.CustomerId, request.CreatedByUserId, request.Term);

        var (items, totalCount) = await _repository.GetPagedAsync(filter, today, request.Page, ct);
        var names = await _userRepository.GetDisplayNamesAsync(EventMapper.UserIds(items), ct);
        var lang = _languageProvider.Language;

        return Result<PagedData<EventDto>>.Success(new PagedData<EventDto>
        {
            Items = items.Select(e => EventMapper.ToDto(e, lang, today, names)).ToList(),
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventQueries.GetEventsReport;

public class GetEventsReportQueryHandler : IRequestHandler<GetEventsReportQuery, Result<EventsReportDto>>
{
    internal const int MaxRows = 2000;

    private readonly IEventRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly ITenantSettingsCurrencyReader _currencyReader;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetEventsReportQueryHandler(
        IEventRepository repository,
        IUserRepository userRepository,
        ICurrentLanguageProvider languageProvider,
        ITenantSettingsCurrencyReader currencyReader)
    {
        _repository = repository;
        _userRepository = userRepository;
        _languageProvider = languageProvider;
        _currencyReader = currencyReader;
    }

    public async Task<Result<EventsReportDto>> Handle(GetEventsReportQuery request, CancellationToken ct)
    {
        var today = TenantCalendar.Today;
        var filter = new EventListFilter(
            request.BranchId, request.From, request.To, request.Status,
            request.CustomerId, request.CreatedByUserId);

        // One extra row tells us whether the cap cut anything off.
        var events = await _repository.GetForReportAsync(filter, today, MaxRows + 1, ct);
        var truncated = events.Count > MaxRows;
        if (truncated) events = events.Take(MaxRows).ToList();

        var names = await _userRepository.GetDisplayNamesAsync(EventMapper.UserIds(events), ct);
        var lang = _languageProvider.Language;
        var rows = events.Select(e => EventMapper.ToDto(e, lang, today, names)).ToList();

        var live = rows.Where(r => r.Status != EventStatus.Cancelled).ToList();
        // Billed/paid cover every row: a cancelled event that kept its deposit still brought that
        // money in (its Billed is what was kept; a fully cancelled one bills 0).
        var totals = new EventsReportTotalsDto(
            EventCount: live.Count,
            CancelledCount: rows.Count - live.Count,
            TotalCapacity: live.Sum(r => r.Capacity),
            DecoratedCount: live.Count(r => r.WithDecorations),
            TotalBilled: rows.Sum(r => r.Billed),
            TotalPaid: rows.Sum(r => r.AmountPaid),
            TotalBalance: rows.Sum(r => r.Balance));

        var currency = rows.FirstOrDefault()?.Currency
            ?? await _currencyReader.GetCurrencyAsync(ct) ?? "KWD";

        return Result<EventsReportDto>.Success(
            new EventsReportDto(request.From, request.To, currency, rows, totals, truncated), _operation);
    }
}

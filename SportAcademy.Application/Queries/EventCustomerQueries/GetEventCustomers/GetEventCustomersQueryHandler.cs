using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomers;

public class GetEventCustomersQueryHandler : IRequestHandler<GetEventCustomersQuery, Result<PagedData<EventCustomerDto>>>
{
    private readonly IEventCustomerRepository _repository;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetEventCustomersQueryHandler(IEventCustomerRepository repository, ICurrentLanguageProvider languageProvider)
    {
        _repository = repository;
        _languageProvider = languageProvider;
    }

    public async Task<Result<PagedData<EventCustomerDto>>> Handle(GetEventCustomersQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page, request.Term, request.NationalityCategoryId, request.IsActive, _languageProvider.Language, ct);

        return Result<PagedData<EventCustomerDto>>.Success(new PagedData<EventCustomerDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}

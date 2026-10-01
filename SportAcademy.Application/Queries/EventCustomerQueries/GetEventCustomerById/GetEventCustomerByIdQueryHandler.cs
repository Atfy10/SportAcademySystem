using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerById;

public class GetEventCustomerByIdQueryHandler : IRequestHandler<GetEventCustomerByIdQuery, Result<EventCustomerDto>>
{
    private readonly IEventCustomerRepository _repository;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly string _operation = OperationType.Get.ToString();

    public GetEventCustomerByIdQueryHandler(IEventCustomerRepository repository, ICurrentLanguageProvider languageProvider)
    {
        _repository = repository;
        _languageProvider = languageProvider;
    }

    public async Task<Result<EventCustomerDto>> Handle(GetEventCustomerByIdQuery request, CancellationToken ct)
    {
        var dto = await _repository.GetSummaryAsync(request.Id, _languageProvider.Language, ct)
            ?? throw new EventCustomerNotFoundException(request.Id.ToString());

        return Result<EventCustomerDto>.Success(dto, _operation);
    }
}

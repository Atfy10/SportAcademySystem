using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerByPhone;

public class GetEventCustomerByPhoneQueryHandler : IRequestHandler<GetEventCustomerByPhoneQuery, Result<EventCustomerDto?>>
{
    private readonly IEventCustomerRepository _repository;
    private readonly IPhoneNumberNormalizer _phoneNormalizer;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly string _operation = OperationType.Get.ToString();

    public GetEventCustomerByPhoneQueryHandler(
        IEventCustomerRepository repository,
        IPhoneNumberNormalizer phoneNormalizer,
        ICurrentLanguageProvider languageProvider)
    {
        _repository = repository;
        _phoneNormalizer = phoneNormalizer;
        _languageProvider = languageProvider;
    }

    public async Task<Result<EventCustomerDto?>> Handle(GetEventCustomerByPhoneQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Phone))
            return Result<EventCustomerDto?>.Success(null, _operation);

        // Same normalization the customer was stored with, so any way of typing the number finds them.
        var phone = await _phoneNormalizer.NormalizeAsync(request.Phone, ct) ?? request.Phone.Trim();
        var customer = await _repository.GetByPhoneAsync(phone, ct);

        var dto = customer is null
            ? null
            : await _repository.GetSummaryAsync(customer.Id, _languageProvider.Language, ct);

        return Result<EventCustomerDto?>.Success(dto, _operation);
    }
}

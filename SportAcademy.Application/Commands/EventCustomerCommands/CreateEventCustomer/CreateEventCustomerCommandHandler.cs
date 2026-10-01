using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Commands.EventCustomerCommands.CreateEventCustomer
{
    public class CreateEventCustomerCommandHandler : IRequestHandler<CreateEventCustomerCommand, Result<EventCustomerDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IEventCustomerRepository _repository;
        private readonly INationalityCategoryRepository _nationalityCategoryRepository;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly ICurrentLanguageProvider _languageProvider;

        public CreateEventCustomerCommandHandler(
            IEventCustomerRepository repository,
            INationalityCategoryRepository nationalityCategoryRepository,
            IPhoneNumberNormalizer phoneNormalizer,
            ICurrentLanguageProvider languageProvider)
        {
            _repository = repository;
            _nationalityCategoryRepository = nationalityCategoryRepository;
            _phoneNormalizer = phoneNormalizer;
            _languageProvider = languageProvider;
        }

        public async Task<Result<EventCustomerDto>> Handle(CreateEventCustomerCommand request, CancellationToken cancellationToken)
        {
            // Normalized before the uniqueness check, so "5xxxxxxx" and "+965 5xxxxxxx" are the
            // same customer.
            var phone = await _phoneNormalizer.NormalizeAsync(request.PhoneNumber, cancellationToken) ?? request.PhoneNumber.Trim();

            if (await _repository.IsPhoneTakenAsync(phone, null, cancellationToken))
                throw EventRuleException.CustomerPhoneExists(phone);

            if (!await _nationalityCategoryRepository.IsExistAsync(request.NationalityCategoryId, cancellationToken))
                throw new IdNotFoundException(nameof(NationalityCategory), request.NationalityCategoryId);

            var customer = new EventCustomer
            {
                FullName = request.FullName.Trim(),
                PhoneNumber = phone,
                NationalityCategoryId = request.NationalityCategoryId,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            };

            await _repository.AddAsync(customer, cancellationToken);

            var dto = await _repository.GetSummaryAsync(customer.Id, _languageProvider.Language, cancellationToken)
                ?? throw new EventCustomerNotFoundException(customer.Id.ToString());

            return Result<EventCustomerDto>.Success(dto, _operation);
        }
    }
}

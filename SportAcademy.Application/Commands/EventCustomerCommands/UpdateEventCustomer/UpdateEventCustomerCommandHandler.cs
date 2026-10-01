using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Commands.EventCustomerCommands.UpdateEventCustomer
{
    // Invoices already issued keep the name/phone they were issued under (Invoice.PayerName/
    // PayerPhone) - editing the customer changes future bookings, never past receipts.
    public class UpdateEventCustomerCommandHandler : IRequestHandler<UpdateEventCustomerCommand, Result<EventCustomerDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventCustomerRepository _repository;
        private readonly INationalityCategoryRepository _nationalityCategoryRepository;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly ICurrentLanguageProvider _languageProvider;

        public UpdateEventCustomerCommandHandler(
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

        public async Task<Result<EventCustomerDto>> Handle(UpdateEventCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EventCustomerNotFoundException(request.Id.ToString());

            var phone = await _phoneNormalizer.NormalizeAsync(request.PhoneNumber, cancellationToken) ?? request.PhoneNumber.Trim();

            if (phone != customer.PhoneNumber && await _repository.IsPhoneTakenAsync(phone, customer.Id, cancellationToken))
                throw EventRuleException.CustomerPhoneExists(phone);

            if (request.NationalityCategoryId != customer.NationalityCategoryId
                && !await _nationalityCategoryRepository.IsExistAsync(request.NationalityCategoryId, cancellationToken))
                throw new IdNotFoundException(nameof(NationalityCategory), request.NationalityCategoryId);

            customer.FullName = request.FullName.Trim();
            customer.PhoneNumber = phone;
            customer.NationalityCategoryId = request.NationalityCategoryId;
            customer.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            customer.IsActive = request.IsActive;

            await _repository.UpdateAsync(customer, cancellationToken);

            var dto = await _repository.GetSummaryAsync(customer.Id, _languageProvider.Language, cancellationToken)
                ?? throw new EventCustomerNotFoundException(customer.Id.ToString());

            return Result<EventCustomerDto>.Success(dto, _operation);
        }
    }
}

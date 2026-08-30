using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType
{
    public class UpdatePaymentTypeCommandHandler : IRequestHandler<UpdatePaymentTypeCommand, Result<PaymentTypeDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IPaymentTypeRepository _repository;
        private readonly ICurrentLanguageProvider _language;

        public UpdatePaymentTypeCommandHandler(IPaymentTypeRepository repository, ICurrentLanguageProvider language)
        {
            _repository = repository;
            _language = language;
        }

        public async Task<Result<PaymentTypeDto>> Handle(UpdatePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdWithTranslationsTrackedAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<PaymentTypeDto>.Failure($"Payment type with ID {request.Id} not found.", _operation, 404);

            if (request.Name is not null)
                entity.Name = request.Name;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            // Setting this one as default clears whichever other type held it - exactly one
            // default per tenant. Explicitly un-defaulting the current default is a no-op here;
            // the tenant simply keeps its default until another type is flagged instead
            // (mirrors CreatePaymentTypeCommandHandler's "never leave zero defaults" guarantee).
            if (request.IsDefault is true && !entity.IsDefault)
            {
                await _repository.ClearDefaultFlagAsync(entity.Id, cancellationToken);
                entity.IsDefault = true;
            }

            // Patch-style, matching every other field on this command: null means "not part of
            // this request", empty string means "clear it", non-empty means "set it".
            if (request.NameAr is not null)
            {
                var trimmedName = request.NameAr.Trim();
                var existingTranslation = entity.Translations.FirstOrDefault(t => t.LangCode == "ar");

                if (trimmedName.Length == 0)
                {
                    if (existingTranslation is not null) entity.Translations.Remove(existingTranslation);
                }
                else if (existingTranslation is not null)
                {
                    existingTranslation.Name = trimmedName;
                }
                else
                {
                    entity.Translations.Add(new PaymentTypeTranslation { LangCode = "ar", Name = trimmedName });
                }
            }

            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<PaymentTypeDto>.Success(entity.ToDto(_language.Language), _operation);
        }
    }
}

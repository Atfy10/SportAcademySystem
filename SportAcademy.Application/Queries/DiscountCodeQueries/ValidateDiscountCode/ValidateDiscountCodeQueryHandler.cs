using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.DiscountCodeExceptions;

namespace SportAcademy.Application.Queries.DiscountCodeQueries.ValidateDiscountCode
{
    // Used by SubscriptionFormModal's live preview before a discount request is actually
    // submitted - shares GetActiveByCodeAsync with the authoritative checks in
    // CreateSubscriptionDiscountRequestCommandHandler/ApproveSubscriptionDiscountRequestCommandHandler,
    // so "what the frontend previews" and "what the backend actually honors" can never disagree.
    public class ValidateDiscountCodeQueryHandler : IRequestHandler<ValidateDiscountCodeQuery, Result<DiscountCodeValidationDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly IDiscountCodeRepository _repository;

        public ValidateDiscountCodeQueryHandler(IDiscountCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<DiscountCodeValidationDto>> Handle(ValidateDiscountCodeQuery request, CancellationToken cancellationToken)
        {
            var normalized = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
            var code = await _repository.GetActiveByCodeAsync(normalized, cancellationToken)
                ?? throw new InvalidDiscountCodeException(request.Code ?? string.Empty);

            return Result<DiscountCodeValidationDto>.Success(
                new DiscountCodeValidationDto(code.Id, code.Code, code.PercentageOff), _operation);
        }
    }
}

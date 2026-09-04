using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.CreateDiscountCode
{
    public class CreateDiscountCodeCommandHandler : IRequestHandler<CreateDiscountCodeCommand, Result<DiscountCodeDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IDiscountCodeRepository _repository;

        public CreateDiscountCodeCommandHandler(IDiscountCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<DiscountCodeDto>> Handle(CreateDiscountCodeCommand request, CancellationToken cancellationToken)
        {
            var entity = new DiscountCode
            {
                Code = request.Code.Trim().ToUpperInvariant(),
                Description = request.Description,
                PercentageOff = request.PercentageOff,
                IsActive = request.IsActive,
                ExpiresAt = request.ExpiresAt,
            };

            await _repository.AddAsync(entity, cancellationToken);

            return Result<DiscountCodeDto>.Success(
                new DiscountCodeDto(entity.Id, entity.Code, entity.Description, entity.PercentageOff, entity.IsActive, entity.ExpiresAt),
                _operation);
        }
    }
}

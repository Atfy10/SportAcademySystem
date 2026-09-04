using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.DiscountCodeExceptions;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.UpdateDiscountCode
{
    public class UpdateDiscountCodeCommandHandler : IRequestHandler<UpdateDiscountCodeCommand, Result<DiscountCodeDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IDiscountCodeRepository _repository;

        public UpdateDiscountCodeCommandHandler(IDiscountCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<DiscountCodeDto>> Handle(UpdateDiscountCodeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new DiscountCodeNotFoundException(request.Id.ToString());

            entity.Code = request.Code.Trim().ToUpperInvariant();
            entity.Description = request.Description;
            entity.PercentageOff = request.PercentageOff;
            entity.IsActive = request.IsActive;
            entity.ExpiresAt = request.ExpiresAt;

            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<DiscountCodeDto>.Success(
                new DiscountCodeDto(entity.Id, entity.Code, entity.Description, entity.PercentageOff, entity.IsActive, entity.ExpiresAt),
                _operation);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.DiscountCodeExceptions;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.DeleteDiscountCode
{
    public class DeleteDiscountCodeCommandHandler : IRequestHandler<DeleteDiscountCodeCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IDiscountCodeRepository _repository;

        public DeleteDiscountCodeCommandHandler(IDiscountCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteDiscountCodeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new DiscountCodeNotFoundException(request.Id.ToString());

            if (await _repository.HasInvoiceLinesAsync(entity.Id, cancellationToken))
                throw new DiscountCodeInUseException(entity.Id);

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

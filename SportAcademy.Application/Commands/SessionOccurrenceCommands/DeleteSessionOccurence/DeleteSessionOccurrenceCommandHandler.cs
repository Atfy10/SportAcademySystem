using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence
{
    public class DeleteSessionOccurrenceCommandHandler : IRequestHandler<DeleteSessionOccurrenceCommand, Result<bool>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteSessionOccurrenceCommandHandler(ISessionOccurrenceRepository sessionOccurrenceRepository)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
        }

        public async Task<Result<bool>> Handle(DeleteSessionOccurrenceCommand request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = await _sessionOccurrenceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _sessionOccurrenceRepository.DeleteAsync(sessionOccurrence, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence
{
    public class UpdateSessionOccurrenceCommandHandler : IRequestHandler<UpdateSessionOccurrenceCommand, Result<SessionOccurrenceDto>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateSessionOccurrenceCommandHandler(
            ISessionOccurrenceRepository sessionOccurrenceRepository)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
        }

        public async Task<Result<SessionOccurrenceDto>> Handle(UpdateSessionOccurrenceCommand request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = await _sessionOccurrenceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException($"{request.Id}");

            sessionOccurrence.Update(request.StartDateTime, request.Status);

            cancellationToken.ThrowIfCancellationRequested();

            await _sessionOccurrenceRepository.UpdateAsync(sessionOccurrence, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var sessionOccurrenceDto = sessionOccurrence.ToDto();

            return Result<SessionOccurrenceDto>.Success(sessionOccurrenceDto, _operationType);
        }
    }
}

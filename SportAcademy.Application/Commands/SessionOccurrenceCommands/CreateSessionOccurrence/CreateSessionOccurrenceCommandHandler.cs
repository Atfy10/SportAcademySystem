using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence
{
    public class CreateSessionOccurrenceCommandHandler : IRequestHandler<CreateSessionOccurrenceCommand, Result<int>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateSessionOccurrenceCommandHandler(
            ISessionOccurrenceRepository sessionOccurrenceRepository)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
        }

        public async Task<Result<int>> Handle(CreateSessionOccurrenceCommand request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = request.ToSessionOccurrence();

            cancellationToken.ThrowIfCancellationRequested();

            await _sessionOccurrenceRepository.AddAsync(sessionOccurrence, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(sessionOccurrence.Id, _operationType);
        }
    }
}

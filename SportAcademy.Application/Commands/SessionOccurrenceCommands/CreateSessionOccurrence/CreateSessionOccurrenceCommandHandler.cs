using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence
{
    public class CreateSessionOccurrenceCommandHandler : IRequestHandler<CreateSessionOccurrenceCommand, Result<int>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateSessionOccurrenceCommandHandler(
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            IMapper mapper)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateSessionOccurrenceCommand request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = _mapper.Map<SessionOccurrence>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            cancellationToken.ThrowIfCancellationRequested();

            await _sessionOccurrenceRepository.AddAsync(sessionOccurrence, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(sessionOccurrence.Id, _operationType);
        }
    }
}

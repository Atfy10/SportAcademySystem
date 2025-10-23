using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence
{
    public class UpdateSessionOccurrenceCommandHandler : IRequestHandler<UpdateSessionOccurrenceCommand, Result<SessionOccurrenceDto>>
    {
        private readonly IMapper _mapper;
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateSessionOccurrenceCommandHandler(
            IMapper mapper,
            ISessionOccurrenceRepository sessionOccurrenceRepository)
        {
            _mapper = mapper;
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
        }

        public async Task<Result<SessionOccurrenceDto>> Handle(UpdateSessionOccurrenceCommand request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = await _sessionOccurrenceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException($"{request.Id}");

            _mapper.Map(request, sessionOccurrence);

            cancellationToken.ThrowIfCancellationRequested();

            await _sessionOccurrenceRepository.UpdateAsync(sessionOccurrence, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var sessionOccurrenceDto = _mapper.Map<SessionOccurrenceDto>(sessionOccurrence)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<SessionOccurrenceDto>.Success(sessionOccurrenceDto, _operationType);
        }
    }
}

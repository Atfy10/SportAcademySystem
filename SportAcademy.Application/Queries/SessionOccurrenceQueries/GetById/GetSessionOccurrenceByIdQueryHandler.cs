using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetById
{
    public class GetSessionOccurrenceByIdQueryHandler : IRequestHandler<GetSessionOccurrenceByIdQuery, Result<SessionOccurrenceDto>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSessionOccurrenceByIdQueryHandler(
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            IMapper mapper)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _mapper = mapper;
        }

        public async Task<Result<SessionOccurrenceDto>> Handle(GetSessionOccurrenceByIdQuery request, CancellationToken cancellationToken)
        {
            var sessionOccurrence = await _sessionOccurrenceRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SessionOccurrenceNotFoundException($"{request.Id}");

            var sessionOccurrenceDto = _mapper.Map<SessionOccurrenceDto>(sessionOccurrence)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<SessionOccurrenceDto>.Success(sessionOccurrenceDto, _operationType);
        }
    }
}

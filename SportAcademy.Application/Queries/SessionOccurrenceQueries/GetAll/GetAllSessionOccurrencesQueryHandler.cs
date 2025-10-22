using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetAll
{
    public class GetAllSessionOccurrencesQueryHandler : IRequestHandler<GetAllSessionOccurrencesQuery, Result<List<SessionOccurrenceDto>>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllSessionOccurrencesQueryHandler(
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            IMapper mapper)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<SessionOccurrenceDto>>> Handle(GetAllSessionOccurrencesQuery request, CancellationToken cancellationToken)
        {
            var sessionOccurrences = await _sessionOccurrenceRepository.GetAllAsync(cancellationToken) 
                ?? [];

            var sessionOccurrencesDto = _mapper.Map<List<SessionOccurrenceDto>>(sessionOccurrences) 
                ?? [];

            return Result<List<SessionOccurrenceDto>>.Success(sessionOccurrencesDto, _operationType);
        }
    }
}

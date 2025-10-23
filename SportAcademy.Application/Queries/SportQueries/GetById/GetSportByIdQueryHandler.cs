using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Queries.SportQueries.GetById
{
    public class GetSportByIdQueryHandler : IRequestHandler<GetSportByIdQuery, Result<SportDto>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSportByIdQueryHandler(
            ISportRepository sportRepository,
            IMapper mapper)
        {
            _sportRepository = sportRepository;
            _mapper = mapper;
        }

        public async Task<Result<SportDto>> Handle(GetSportByIdQuery request, CancellationToken cancellationToken)
        {
            var sport = await _sportRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SportNotFoundException($"{request.Id}");

            var sportDto = _mapper.Map<SportDto>(sport)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<SportDto>.Success(sportDto, _operationType);
        }
    }
}

using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.CoachQueries.GetById
{
    public class GetCoachByIdQueryHandler : IRequestHandler<GetCoachByIdQuery, Result<CoachDetailsDto>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly IMapper _mapper;

        public GetCoachByIdQueryHandler(
            ICoachRepository coachRepository,
            IMapper mapper)
        {
            _coachRepository = coachRepository;
            _mapper = mapper;
        }

        public async Task<Result<CoachDetailsDto>> Handle(GetCoachByIdQuery request, CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            
            if (coach == null)
                return Result<CoachDetailsDto>.Failure(nameof(GetCoachByIdQuery), $"Coach with ID {request.Id} not found");

            var coachDetailsDto = _mapper.Map<CoachDetailsDto>(coach);
            
            return Result<CoachDetailsDto>.Success(coachDetailsDto, nameof(GetCoachByIdQuery));
        }
    }
}

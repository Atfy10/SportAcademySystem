using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;

namespace SportAcademy.Application.Queries.CoachQueries.GetById
{
    public class GetCoachByIdQueryHandler : IRequestHandler<GetCoachByIdQuery, Result<CoachDetailsDto>>
    {
        private readonly ICoachRepository _coachRepository;

        public GetCoachByIdQueryHandler(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }

        public async Task<Result<CoachDetailsDto>> Handle(GetCoachByIdQuery request, CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

            if (coach == null)
                return Result<CoachDetailsDto>.Failure(nameof(GetCoachByIdQuery), $"Coach with ID {request.Id} not found");

            var coachDetailsDto = coach.ToCoachDetailsDto();

            return Result<CoachDetailsDto>.Success(coachDetailsDto, nameof(GetCoachByIdQuery));
        }
    }
}

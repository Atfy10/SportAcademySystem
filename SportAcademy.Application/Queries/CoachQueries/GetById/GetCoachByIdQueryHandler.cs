using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Queries.CoachQueries.GetById
{
    public class GetCoachByIdQueryHandler : IRequestHandler<GetCoachByIdQuery, Result<CoachDetailsDto>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetCoachByIdQueryHandler(
            ICoachRepository coachRepository,
            IMapper mapper,
            ICurrentLanguageProvider languageProvider)
        {
            _coachRepository = coachRepository;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<Result<CoachDetailsDto>> Handle(GetCoachByIdQuery request, CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

            if (coach == null)
                return Result<CoachDetailsDto>.Failure(nameof(GetCoachByIdQuery), $"Coach with ID {request.Id} not found");

            var coachDetailsDto = _mapper.Map<CoachDetailsDto>(coach);

            // AutoMapper's ConstructUsing runs in-memory here (Map, not ProjectTo), but the
            // profile itself has no per-request language to splice in - Branch/Sport were
            // already eagerly loaded with Translations by GetByIdWithDetailsAsync, so resolve
            // the override the same way the other translated entities' detail handlers do.
            var lang = _languageProvider.Language;
            coachDetailsDto = coachDetailsDto with
            {
                BranchName = coach.Employee.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? coachDetailsDto.BranchName,
                SportName = coach.Sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? coachDetailsDto.SportName,
            };

            return Result<CoachDetailsDto>.Success(coachDetailsDto, nameof(GetCoachByIdQuery));
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.FamilyQueries.GetFamilyById
{
    public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDetailDto>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetFamilyByIdQueryHandler(IFamilyRepository familyRepository, ICurrentLanguageProvider languageProvider)
        {
            _familyRepository = familyRepository;
            _languageProvider = languageProvider;
        }

        public async Task<Result<FamilyDetailDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
        {
            var family = await _familyRepository.GetByIdProjectedAsync<FamilyDetailDto>(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.Id.ToString());

            // Same overlay pattern as GetTraineeGroupByIdQueryHandler: the Members list's
            // Age/IsSubscribed computation must stay on AutoMapper's ProjectTo path, so only
            // Family's own Name/GuardianName are resolved with a second small query.
            var translated = await _familyRepository.GetTranslatedNamesAsync(request.Id, _languageProvider.Language, cancellationToken);
            if (translated is not null)
            {
                family = family with
                {
                    Name = translated.Value.Name ?? family.Name,
                    GuardianName = translated.Value.GuardianName ?? family.GuardianName,
                };
            }

            return Result<FamilyDetailDto>.Success(family, nameof(GetFamilyByIdQuery));
        }
    }
}

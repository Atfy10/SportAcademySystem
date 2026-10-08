using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.FamilyQueries.SearchFamily
{
    public class SearchFamilyQueryHandler : IRequestHandler<SearchFamilyQuery, Result<IReadOnlyList<FamilyDto>>>
    {
        private const int MaxResults = 20;
        private readonly IFamilyRepository _familyRepository;
        public SearchFamilyQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }
        public async Task<Result<IReadOnlyList<FamilyDto>>> Handle(SearchFamilyQuery request, CancellationToken cancellationToken)
        {
            // Front-desk staff know a family by the guardian's name or phone, or a sibling's name -
            // rarely by its numeric code. Matching only the code (and throwing on anything else)
            // made the trainee form's family picker useless, so an empty result is a normal answer
            // now, not an error.
            var families = await _familyRepository.SearchFamiliesTranslatedAsync(request.Term, MaxResults, cancellationToken);
            return Result<IReadOnlyList<FamilyDto>>.Success(families, nameof(SearchFamilyQuery));
        }
    }
}
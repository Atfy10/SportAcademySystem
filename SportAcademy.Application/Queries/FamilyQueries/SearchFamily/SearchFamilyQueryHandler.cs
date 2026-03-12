using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.FamilyQueries.SearchFamily
{
    public class SearchFamilyQueryHandler : IRequestHandler<SearchFamilyQuery, Result<IReadOnlyList<FamilyDto>>>
    {
        private readonly IFamilyRepository _familyRepository;
        public SearchFamilyQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }
        public async Task<Result<IReadOnlyList<FamilyDto>>> Handle(SearchFamilyQuery request, CancellationToken cancellationToken)
        {
            var searchTerm = request.Term.Trim();
            if (!int.TryParse(searchTerm, out int code))
                throw new Exception("Invalid search term. Please enter a valid family code.");

            var families = await _familyRepository.SearchFamiliesWithCode<FamilyDto>(code, cancellationToken);
            if (families == null || !families.Any())
                throw new Exception("No families found with the provided code.");

            return Result<IReadOnlyList<FamilyDto>>.Success(families, nameof(SearchFamilyQuery));
        }
    }
}
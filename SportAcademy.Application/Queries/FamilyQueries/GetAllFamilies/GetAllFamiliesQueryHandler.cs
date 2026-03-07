using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.FamilyQueries.GetAllFamilies
{
    public class GetAllFamiliesQueryHandler : IRequestHandler<GetAllFamiliesQuery, Result<PagedData<FamilyDto>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetAllFamiliesQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<Result<PagedData<FamilyDto>>> Handle(GetAllFamiliesQuery request, CancellationToken cancellationToken)
        {
            var families = await _familyRepository.GetAllPaginatedAsync<FamilyDto>(request.PageRequest, cancellationToken);

            return Result<PagedData<FamilyDto>>.Success(families, nameof(GetAllFamiliesQuery));
        }
    }
}

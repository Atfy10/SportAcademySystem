using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.FamilyQueries.GetFamilyById
{
    public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDetailDto>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetFamilyByIdQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<Result<FamilyDetailDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
        {
            var family = await _familyRepository.GetByIdProjectedAsync<FamilyDetailDto>(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.Id.ToString());

            return Result<FamilyDetailDto>.Success(family, nameof(GetFamilyByIdQuery));
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.NationalityCategoryQueries.GetAll
{
    public class GetAllQueryHandler : IRequestHandler<GetAllQuery, Result<IReadOnlyList<NationalityCategoryDto>>>
    {
        private readonly INationalityCategoryRepository _nationalityCategoryRepository;

        public GetAllQueryHandler(INationalityCategoryRepository nationalityCategoryRepository)
        {
            _nationalityCategoryRepository = nationalityCategoryRepository;
        }

        public async Task<Result<IReadOnlyList<NationalityCategoryDto>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var nationalities = await _nationalityCategoryRepository.GetAllAsync();
            var nationalityDtos = nationalities.Select(n => new NationalityCategoryDto
            {
                Id = n.Id,
                Code = n.Code,
                Name = n.Name
            }).ToList();

            return Result<IReadOnlyList<NationalityCategoryDto>>.Success(nationalityDtos, nameof(GetAllQuery));
        }
    }
}

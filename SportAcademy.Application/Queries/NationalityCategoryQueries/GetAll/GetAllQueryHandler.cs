using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Queries.NationalityCategoryQueries.GetAll
{
    public class GetAllQueryHandler : IRequestHandler<GetAllQuery, Result<IReadOnlyList<NationalityCategoryDto>>>
    {
        private readonly INationalityCategoryRepository _nationalityCategoryRepository;
        private readonly ICurrentLanguageProvider _language;

        public GetAllQueryHandler(INationalityCategoryRepository nationalityCategoryRepository, ICurrentLanguageProvider language)
        {
            _nationalityCategoryRepository = nationalityCategoryRepository;
            _language = language;
        }

        public async Task<Result<IReadOnlyList<NationalityCategoryDto>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var nationalityDtos = await _nationalityCategoryRepository.GetAllTranslatedAsync(_language.Language, cancellationToken);

            return Result<IReadOnlyList<NationalityCategoryDto>>.Success(nationalityDtos, nameof(GetAllQuery));
        }
    }
}

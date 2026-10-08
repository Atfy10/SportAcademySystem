using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.TranslationQueries.GetArabicTranslation;

public class GetArabicTranslationQueryHandler : IRequestHandler<GetArabicTranslationQuery, Result<ArabicTranslationDto>>
{
    private const string Arabic = "ar";
    private readonly IBranchRepository _branchRepository;
    private readonly ISportRepository _sportRepository;
    private readonly ITraineeGroupRepository _traineeGroupRepository;
    private readonly string _operationType = OperationType.Get.ToString();

    public GetArabicTranslationQueryHandler(
        IBranchRepository branchRepository,
        ISportRepository sportRepository,
        ITraineeGroupRepository traineeGroupRepository)
    {
        _branchRepository = branchRepository;
        _sportRepository = sportRepository;
        _traineeGroupRepository = traineeGroupRepository;
    }

    public async Task<Result<ArabicTranslationDto>> Handle(GetArabicTranslationQuery request, CancellationToken ct)
    {
        ArabicTranslationDto dto;
        switch (request.Entity)
        {
            case TranslatableEntity.Branch:
            {
                var branch = await _branchRepository.GetByIdWithSportsAsync(request.Id, ct)
                    ?? throw new IdNotFoundException("Branch", request.Id.ToString());
                var ar = branch.Translations.FirstOrDefault(t => t.LangCode == Arabic);
                dto = new ArabicTranslationDto(ar?.Name, ar?.City, ar?.Country, null);
                break;
            }
            case TranslatableEntity.Sport:
            {
                var sport = await _sportRepository.GetByIdWithTranslationsAsync(request.Id, ct)
                    ?? throw new IdNotFoundException("Sport", request.Id.ToString());
                var ar = sport.Translations.FirstOrDefault(t => t.LangCode == Arabic);
                dto = new ArabicTranslationDto(ar?.Name, null, null, ar?.Description);
                break;
            }
            case TranslatableEntity.TraineeGroup:
            {
                var group = await _traineeGroupRepository.GetByIdWithTranslationsAsync(request.Id, ct)
                    ?? throw new IdNotFoundException("TraineeGroup", request.Id.ToString());
                var ar = group.Translations.FirstOrDefault(t => t.LangCode == Arabic);
                dto = new ArabicTranslationDto(ar?.Name, null, null, null);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.Entity, "Unknown entity.");
        }

        return Result<ArabicTranslationDto>.Success(dto, _operationType);
    }
}

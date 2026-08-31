using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.FamilyCommands.UpdateFamily
{
    public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, Result<FamilyDto>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateFamilyCommandHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<Result<FamilyDto>> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
        {
            var family = await _familyRepository.GetByIdWithTranslationsAsync(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.Id.ToString());

            family.Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
            family.GuardianName = string.IsNullOrWhiteSpace(request.GuardianName) ? null : request.GuardianName.Trim();
            family.GuardianPhone = string.IsNullOrWhiteSpace(request.GuardianPhone) ? null : request.GuardianPhone.Trim();

            // NameAr/GuardianNameAr == null: leave that field of any existing translation
            // untouched. "" (after trim): explicit clear. Non-empty: upsert. If both fields end
            // up empty on the translation row, drop the row entirely (nothing left to fall back
            // from). Mirrors UpdateSportCommandHandler's per-field null/""/value contract.
            if (request.NameAr is not null || request.GuardianNameAr is not null)
            {
                var existing = family.Translations.FirstOrDefault(t => t.LangCode == "ar");
                var nextName = request.NameAr is null
                    ? existing?.Name
                    : (request.NameAr.Trim().Length == 0 ? null : request.NameAr.Trim());
                var nextGuardianName = request.GuardianNameAr is null
                    ? existing?.GuardianName
                    : (request.GuardianNameAr.Trim().Length == 0 ? null : request.GuardianNameAr.Trim());

                if (nextName is null && nextGuardianName is null)
                {
                    if (existing is not null) family.Translations.Remove(existing);
                }
                else if (existing is not null)
                {
                    existing.Name = nextName;
                    existing.GuardianName = nextGuardianName;
                }
                else
                {
                    family.Translations.Add(new FamilyTranslation
                    {
                        LangCode = "ar",
                        Name = nextName,
                        GuardianName = nextGuardianName,
                    });
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _familyRepository.UpdateAsync(family, cancellationToken);

            var familyDto = await _familyRepository.GetByIdTranslatedAsync(family.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", family.Id.ToString());

            return Result<FamilyDto>.Success(familyDto, _operationType);
        }
    }
}

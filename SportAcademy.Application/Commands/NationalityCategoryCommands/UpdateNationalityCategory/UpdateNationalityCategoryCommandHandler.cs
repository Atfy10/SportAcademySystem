using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.NationalityCategoryExceptions;

namespace SportAcademy.Application.Commands.NationalityCategoryCommands.UpdateNationalityCategory
{
    public class UpdateNationalityCategoryCommandHandler : IRequestHandler<UpdateNationalityCategoryCommand, Result<int>>
    {
        private readonly INationalityCategoryRepository _repository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateNationalityCategoryCommandHandler(INationalityCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<int>> Handle(UpdateNationalityCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdWithTranslationsAsync(request.Id, cancellationToken)
                ?? throw new NationalityCategoryNotFoundException($"{request.Id}");

            if (!entity.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)
                && await _repository.IsCodeExistAsync(request.Code, request.Id, cancellationToken))
                throw new NationalityCategoryCodeExistsException();

            if (!entity.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)
                && await _repository.IsNameExistAsync(request.Name, request.Id, cancellationToken))
                throw new NationalityCategoryNameExistsException();

            entity.Code = request.Code;
            entity.Name = request.Name;

            // NameAr == null: leave any existing translation untouched.
            // NameAr == "" (after trim): explicit clear -> delete the translation row.
            // NameAr non-empty: upsert with the given name.
            if (request.NameAr is not null)
            {
                var trimmedName = request.NameAr.Trim();
                var existingTranslation = entity.Translations.FirstOrDefault(t => t.LangCode == "ar");

                if (trimmedName.Length == 0)
                {
                    if (existingTranslation is not null) entity.Translations.Remove(existingTranslation);
                }
                else if (existingTranslation is not null)
                {
                    existingTranslation.Name = trimmedName;
                }
                else
                {
                    entity.Translations.Add(new NationalityCategoryTranslation { LangCode = "ar", Name = trimmedName });
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<int>.Success(entity.Id, _operationType);
        }
    }
}

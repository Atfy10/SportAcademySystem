using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.NationalityCategoryExceptions;

namespace SportAcademy.Application.Commands.NationalityCategoryCommands.CreateNationalityCategory
{
    public class CreateNationalityCategoryCommandHandler : IRequestHandler<CreateNationalityCategoryCommand, Result<int>>
    {
        private readonly INationalityCategoryRepository _repository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateNationalityCategoryCommandHandler(INationalityCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<int>> Handle(CreateNationalityCategoryCommand request, CancellationToken cancellationToken)
        {
            if (await _repository.IsCodeExistAsync(request.Code, cancellationToken: cancellationToken))
                throw new NationalityCategoryCodeExistsException();

            if (await _repository.IsNameExistAsync(request.Name, cancellationToken: cancellationToken))
                throw new NationalityCategoryNameExistsException();

            var entity = new NationalityCategory
            {
                Code = request.Code,
                Name = request.Name,
            };

            // New entity - attaching the translation to the nav collection here (before
            // AddAsync) makes EF cascade-insert it in the same call.
            if (!string.IsNullOrWhiteSpace(request.NameAr))
            {
                entity.Translations = new List<NationalityCategoryTranslation>
                {
                    new() { LangCode = "ar", Name = request.NameAr.Trim() }
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _repository.AddAsync(entity, cancellationToken);

            return Result<int>.Success(entity.Id, _operationType);
        }
    }
}

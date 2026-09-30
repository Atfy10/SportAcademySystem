using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.FamilyCommands.DeleteFamily
{
    /// <summary>
    /// Deletes an empty family. Soft delete (Family is ISoftDeletable): deleted trainees keep
    /// their required FamilyId, so the row has to stay for them. A family with any active member
    /// is refused - move or delete the members first.
    /// </summary>
    public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand, Result<bool>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteFamilyCommandHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<Result<bool>> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
        {
            var family = await _familyRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.Id.ToString());

            if (await _familyRepository.HasActiveMembersAsync(family.Id, cancellationToken))
                return Result<bool>.Failure(
                    _operationType,
                    "This family still has members. Remove or delete them before deleting the family.",
                    409);

            cancellationToken.ThrowIfCancellationRequested();

            await _familyRepository.DeleteAsync(family, cancellationToken);

            return Result<bool>.Success(true, _operationType);
        }
    }
}

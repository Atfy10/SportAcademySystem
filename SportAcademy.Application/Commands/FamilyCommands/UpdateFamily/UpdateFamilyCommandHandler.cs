using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
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
            var family = await _familyRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.Id.ToString());

            family.Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
            family.GuardianName = string.IsNullOrWhiteSpace(request.GuardianName) ? null : request.GuardianName.Trim();
            family.GuardianPhone = string.IsNullOrWhiteSpace(request.GuardianPhone) ? null : request.GuardianPhone.Trim();

            cancellationToken.ThrowIfCancellationRequested();

            await _familyRepository.UpdateAsync(family, cancellationToken);

            var familyDto = await _familyRepository.GetByIdProjectedAsync<FamilyDto>(family.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", family.Id.ToString());

            return Result<FamilyDto>.Success(familyDto, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.FamilyCommands.AddTraineeToFamily
{
    public class AddTraineeToFamilyCommandHandler : IRequestHandler<AddTraineeToFamilyCommand, Result<FamilyDetailDto>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public AddTraineeToFamilyCommandHandler(IFamilyRepository familyRepository, ITraineeRepository traineeRepository)
        {
            _familyRepository = familyRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<FamilyDetailDto>> Handle(AddTraineeToFamilyCommand request, CancellationToken cancellationToken)
        {
            var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
                ?? throw new IdNotFoundException("Family", request.FamilyId.ToString());

            var trainee = await _traineeRepository.GetFullTrainee(request.TraineeId, cancellationToken)
                ?? throw new IdNotFoundException("Trainee", request.TraineeId.ToString());

            if (trainee.FamilyId != family.Id)
            {
                trainee.FamilyId = family.Id;
                await _traineeRepository.UpdateAsync(trainee, cancellationToken);
            }

            var familyDto = await _familyRepository.GetByIdProjectedAsync<FamilyDetailDto>(family.Id, cancellationToken)
                ?? throw new IdNotFoundException("Family", family.Id.ToString());

            return Result<FamilyDetailDto>.Success(familyDto, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup
{
    public class CreateTraineeGroupCommandHandler : IRequestHandler<CreateTraineeGroupCommand, Result<int>>
    {
        private readonly TraineeGroupService _traineeGroupService;
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeGroupCommandHandler(
            TraineeGroupService traineeGroupService,
            ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupService = traineeGroupService;
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<int>> Handle(CreateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = request.ToTraineeGroup();

            cancellationToken.ThrowIfCancellationRequested();

            var tgName = await _traineeGroupService.GenerateTraineeGroupNameAsync(request);
            traineeGroup.SetName(tgName);

            await _traineeGroupRepository.AddAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(traineeGroup.Id, _operationType);
        }
    }
}

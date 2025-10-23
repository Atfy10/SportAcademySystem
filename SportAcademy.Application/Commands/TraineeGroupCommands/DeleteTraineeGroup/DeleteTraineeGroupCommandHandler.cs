using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.DeleteTraineeGroup
{
    public class DeleteTraineeGroupCommandHandler : IRequestHandler<DeleteTraineeGroupCommand, Result<bool>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteTraineeGroupCommandHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<bool>> Handle(DeleteTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.DeleteAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}

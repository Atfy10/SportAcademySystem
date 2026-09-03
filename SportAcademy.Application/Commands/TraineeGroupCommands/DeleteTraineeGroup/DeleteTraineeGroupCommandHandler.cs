using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.DeleteTraineeGroup
{
    public class DeleteTraineeGroupCommandHandler : IRequestHandler<DeleteTraineeGroupCommand, Result<bool>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteTraineeGroupCommandHandler(
            ITraineeGroupRepository traineeGroupRepository,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.DeleteAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new TraineeGroupDeletedEvent(traineeGroup.Id, traineeGroup.Name, actorName), cancellationToken);

            return Result<bool>.Success(true, _operationType);
        }
    }
}

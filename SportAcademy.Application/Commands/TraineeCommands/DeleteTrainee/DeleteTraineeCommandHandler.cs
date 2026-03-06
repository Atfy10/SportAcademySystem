using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.Trainees.DeleteTrainee
{
    public class DeleteTraineeCommandHandler : IRequestHandler<DeleteTraineeCommand, Result<bool>>
    {
        private readonly IUserContextService _userContextService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly string _operation = OperationType.Delete.ToString();

        public DeleteTraineeCommandHandler(
            ITraineeRepository traineeRepository,
            IUserContextService userContextService)
        {
            _traineeRepository = traineeRepository;
            _userContextService = userContextService;
        }

        public async Task<Result<bool>> Handle(DeleteTraineeCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                throw new ArgumentException("Invalid Id");

            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            trainee.MarkAsDeleted(_userContextService.UserId ?? "System");

            await _traineeRepository.UpdateAsync(trainee, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}

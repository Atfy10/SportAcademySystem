using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public class UpdateTraineePersonalCommandHandler : IRequestHandler<UpdateTraineePersonalCommand, Result<UpdateTraineePersonalCommand>>
    {
        private readonly ITraineeService _traineeService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineePersonalCommandHandler(ITraineeService traineeService,
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _traineeService = traineeService;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }

        public async Task<Result<UpdateTraineePersonalCommand>> Handle(UpdateTraineePersonalCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            _mapper.Map(request, trainee);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.UpdateAsync(trainee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<UpdateTraineePersonalCommand>.Success(request, _operationType);
        }
    }
}

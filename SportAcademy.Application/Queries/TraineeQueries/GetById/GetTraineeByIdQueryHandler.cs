using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Queries.TraineeQueries.GetById
{
    public class GetTraineeByIdQueryHandler : IRequestHandler<GetTraineeByIdQuery, Result<TraineeDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeByIdQueryHandler(ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }
        public async Task<Result<TraineeDto>> Handle(GetTraineeByIdQuery request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                throw new ArgumentException("Id must be greater than zero.");

            cancellationToken.ThrowIfCancellationRequested();

            var trainee = await _traineeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var traineeDto = _mapper.Map<TraineeDto>(trainee)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<TraineeDto>.Success(traineeDto, _operationType);
        }
    }
}

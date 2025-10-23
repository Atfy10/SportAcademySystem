using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee
{
	public class CreateSportTraineeCommandHandler : IRequestHandler<CreateSportTraineeCommand, Result<SportTraineeDto>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly ISportRepository _sportRepository;
		private readonly ITraineeRepository _traineeRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Add.ToString();

		public CreateSportTraineeCommandHandler(
			ISportTraineeRepository sportTraineeRepository,
			ISportRepository sportRepository,
			ITraineeRepository traineeRepository,
			IMapper mapper)
		{
			_sportTraineeRepository = sportTraineeRepository;
			_sportRepository = sportRepository;
			_traineeRepository = traineeRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportTraineeDto>> Handle(CreateSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportTraineeRepository.IsExistAsync(request.SportId, request.TraineeId, cancellationToken);
			if (exists)
				throw new SportTraineeExistsException($"{request.SportId}, {request.TraineeId}");

			var sportExists = await _sportRepository.IsExistAsync(request.SportId, cancellationToken);
			if (!sportExists)
				throw new SportNotFoundException(request.SportId.ToString());

			var traineeExists = await _traineeRepository.IsExistAsync(request.TraineeId, cancellationToken);
			if (!traineeExists)
				throw new TraineeNotFoundException(request.TraineeId.ToString());

			if (!Enum.IsDefined(typeof(SkillLevel), request.SkillLevel))
				throw new InvalidSkillLevelException();

			cancellationToken.ThrowIfCancellationRequested();

			var sportTrainee = _mapper.Map<SportTrainee>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			var dto = _mapper.Map<SportTraineeDto>(sportTrainee)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _sportTraineeRepository.AddAsync(sportTrainee, cancellationToken);
			return Result<SportTraineeDto>.Success(dto, _operationType);
		}

    }
}

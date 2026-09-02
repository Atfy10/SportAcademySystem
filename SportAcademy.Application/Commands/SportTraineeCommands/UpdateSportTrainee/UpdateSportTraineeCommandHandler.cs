using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee
{
	public class UpdateSportTraineeCommandHandler : IRequestHandler<UpdateSportTraineeCommand, Result<SportTraineeDto>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly IMapper _mapper;
		private readonly IPublisher _publisher;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateSportTraineeCommandHandler(
			ISportTraineeRepository sportTraineeRepository,
			IMapper mapper,
			IPublisher publisher)
		{
			_sportTraineeRepository = sportTraineeRepository;
			_mapper = mapper;
			_publisher = publisher;
		}

		public async Task<Result<SportTraineeDto>> Handle(UpdateSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var existing = await _sportTraineeRepository.GetByIdWithIncludesAsync(request.SportId, request.TraineeId, cancellationToken)
				?? throw new SportTraineeNotFoundException($"{request.SportId}, {request.TraineeId}");
			var oldLevel = existing.SkillLevel;

            // See CreateSportTraineeCommandHandler for why this must be a case-insensitive parse.
            if (!Enum.TryParse<SkillLevel>(request.SkillLevel, ignoreCase: true, out var newLevel))
				throw new InvalidSkillLevelException();

			cancellationToken.ThrowIfCancellationRequested();

			// Mutate the already-tracked instance in place rather than mapping a new one -
			// GetByIdWithIncludesAsync above already tracks this (SportId, TraineeId) key, so
			// attaching a second instance with the same key (e.g. via a fresh AutoMapper.Map)
			// throws "cannot be tracked because another instance ... is already being tracked".
			existing.SkillLevel = newLevel;

			var dto = _mapper.Map<SportTraineeDto>(existing)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _sportTraineeRepository.UpdateAsync(existing, cancellationToken);

			if (newLevel != oldLevel)
			{
				await _publisher.Publish(new SportTraineeSkillLevelChangedEvent(
					existing.TraineeId, existing.SportId, oldLevel, newLevel), cancellationToken);
			}

			return Result<SportTraineeDto>.Success(dto, _operationType);
		}
	}
}

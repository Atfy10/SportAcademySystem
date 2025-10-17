using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee
{
	public class UpdateSportTraineeCommandHandler : IRequestHandler<UpdateSportTraineeCommand, Result<SportTraineeDto>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateSportTraineeCommandHandler(
			ISportTraineeRepository sportTraineeRepository,
			IMapper mapper)
		{
			_sportTraineeRepository = sportTraineeRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportTraineeDto>> Handle(UpdateSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportTraineeRepository.IsKeyExist(request.SportId, request.TraineeId, cancellationToken);
			if (!exists)
				throw new SportTraineeNotFoundException($"{request.SportId}, {request.TraineeId}");

            if (!Enum.IsDefined(typeof(SkillLevel), request.SkillLevel))
				throw new InvalidSkillLevelException();

			cancellationToken.ThrowIfCancellationRequested();

			var sportTrainee = _mapper.Map<SportTrainee>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			var dto = _mapper.Map<SportTraineeDto>(sportTrainee)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _sportTraineeRepository.UpdateAsync(sportTrainee, cancellationToken);

			return Result<SportTraineeDto>.Success(dto, _operationType);
		}
	}
}

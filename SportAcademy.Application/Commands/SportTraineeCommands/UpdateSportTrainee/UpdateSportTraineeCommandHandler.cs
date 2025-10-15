using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee
{
	public class UpdateSportTraineeCommandHandler : IRequestHandler<UpdateSportTraineeCommand, Result<string>>
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

		public async Task<Result<string>> Handle(UpdateSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportTraineeRepository.CheckIfKeyExists(request.SportId, request.TraineeId, cancellationToken);
			if (!exists)
				throw new SportTraineeNotFoundException();

			if (!Enum.IsDefined(typeof(SkillLevel), request.SkillLevel))
				throw new InvalidSkillLevelException();

			cancellationToken.ThrowIfCancellationRequested();

			var sportTrainee = _mapper.Map<SportTrainee>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			await _sportTraineeRepository.UpdateAsync(sportTrainee, cancellationToken);

			return Result<string>.Success("SportTrainee updated successfully", _operationType);
		}
	}
}

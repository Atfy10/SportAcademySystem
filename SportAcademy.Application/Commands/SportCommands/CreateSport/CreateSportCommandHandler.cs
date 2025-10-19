using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportCommands.CreateSport
{
	public class CreateSportCommandHandler : IRequestHandler<CreateSportCommand, Result<SportDto>>
	{
		private readonly ISportRepository _sportRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Add.ToString();


		public CreateSportCommandHandler(ISportRepository sportRepository, IMapper mapper)
		{
			_sportRepository = sportRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportDto>> Handle(CreateSportCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportRepository.CheckIfNameExists(request.Name, null, cancellationToken);
			if (exists)
				throw new SportExistsException();

			if (!Enum.IsDefined(typeof(SportCategory), request.Category))
				throw new InvalidCategoryException();

			var sport = _mapper.Map<Sport>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			cancellationToken.ThrowIfCancellationRequested();

			await _sportRepository.AddAsync(sport, cancellationToken);

			var sportDto = _mapper.Map<SportDto>(sport)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			return Result<SportDto>.Success(sportDto, _operationType);
		}

	}
}

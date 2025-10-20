using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportCommands.UpdateSport
{
	public class UpdateSportCommandHandler : IRequestHandler<UpdateSportCommand, Result<SportDto>>
	{
		private readonly ISportRepository _sportRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateSportCommandHandler(ISportRepository sportRepository, IMapper mapper)
		{
			_sportRepository = sportRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportDto>> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
		{
			var sport = await _sportRepository.GetByIdAsync(request.Id, cancellationToken)
				?? throw new SportNotFoundException($"{request.Id}");

			if (!string.IsNullOrWhiteSpace(request.Name))
			{
				var exists = await _sportRepository.CheckIfNameExists(request.Name, cancellationToken);
				if (exists && !sport.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
					throw new SportExistsException();
			}

			sport.Update(request.Name, request.Description, request.Category, request.IsRequireHealthTest);


			await _sportRepository.UpdateAsync(sport, cancellationToken);

			var sportDto = _mapper.Map<SportDto>(sport);
			return Result<SportDto>.Success(sportDto, _operationType);
		}
	}
}

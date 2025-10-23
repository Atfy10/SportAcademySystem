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
        private readonly IMapper _mapper;
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateSportCommandHandler(
            IMapper mapper,
            ISportRepository sportRepository)
        {
            _mapper = mapper;
            _sportRepository = sportRepository;
        }

        public async Task<Result<SportDto>> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
        {
            var sport = await _sportRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SportNotFoundException($"{request.Id}");

            if (!sport.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = await _sportRepository.IsExistByNameAsync(request.Name, cancellationToken);
                if (nameExists)
                    throw new SportExistsException();
            }

            _mapper.Map(request, sport);

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.UpdateAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var sportDto = _mapper.Map<SportDto>(sport)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<SportDto>.Success(sportDto, _operationType);
        }
    }

}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.UpdateSport
{
    public class UpdateSportCommandHandler : IRequestHandler<UpdateSportCommand, Result<SportDto>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateSportCommandHandler(ISportRepository sportRepository)
        {
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

            sport.ApplyUpdate(request);

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.UpdateAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var sportDto = sport.ToDto();

            return Result<SportDto>.Success(sportDto, _operationType);
        }
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.UpdateSport
{
    public class UpdateSportCommandHandler : IRequestHandler<UpdateSportCommand, Result<SportDto>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateSportCommandHandler(
            ISportRepository sportRepository,
            IUnitOfWork unitOfWork)
        {
            _sportRepository = sportRepository;
            _unitOfWork = unitOfWork;
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

            SportMapper.ApplyUpdate(sport, request);

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.UpdateAsyncWithoutSave(sport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<SportDto>.Success(SportMapper.ToDto(sport), _operationType);
        }
    }
}

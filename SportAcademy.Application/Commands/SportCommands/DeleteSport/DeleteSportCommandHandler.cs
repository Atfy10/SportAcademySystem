using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportCommands.DeleteSport
{
    public class DeleteSportCommandHandler : IRequestHandler<DeleteSportCommand, Result<bool>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteSportCommandHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<bool>> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
        {
            var sport = await _sportRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SportNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.DeleteAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}

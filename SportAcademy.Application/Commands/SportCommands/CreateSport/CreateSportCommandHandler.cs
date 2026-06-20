using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.CreateSport
{
    public class CreateSportCommandHandler : IRequestHandler<CreateSportCommand, Result<int>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateSportCommandHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<int>> Handle(CreateSportCommand request, CancellationToken cancellationToken)
        {
            var sport = request.ToSport();

            var nameExists = await _sportRepository.IsExistByNameAsync(sport.Name, cancellationToken);
            if (nameExists)
                throw new SportExistsException();

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.AddAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(sport.Id, _operationType);
        }
    }
}

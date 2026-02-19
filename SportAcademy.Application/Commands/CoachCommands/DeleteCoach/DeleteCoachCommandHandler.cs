using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.CoachCommands.DeleteCoach
{
    public class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, Result<bool>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly IUserContextService _userContextService;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteCoachCommandHandler(
            ICoachRepository coachRepository,
            IUserContextService userContextService)
        {
            _coachRepository = coachRepository;
            _userContextService = userContextService;
        }

        public async Task<Result<bool>> Handle(DeleteCoachCommand request, CancellationToken ct)
        {
            var coach = await _coachRepository.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new IdNotFoundException(nameof(Coach), request.EmployeeId.ToString());

            coach.MarkAsDeleted(_userContextService.UserId ?? "System");
            await _coachRepository.UpdateAsync(coach, ct);

            return Result<bool>.Success(true, _operationType);
        }
    }
}

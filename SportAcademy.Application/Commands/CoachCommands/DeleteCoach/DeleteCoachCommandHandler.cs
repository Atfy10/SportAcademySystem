using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.DeleteCoach
{
    public class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, Result<bool>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteCoachCommandHandler(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }

        public async Task<Result<bool>> Handle(DeleteCoachCommand request, CancellationToken ct)
        {
            var coach = await _coachRepository.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new IdNotFoundException(nameof(Coach), request.EmployeeId.ToString());

            // Soft delete
            coach.IsDeleted = true;
            coach.DeletedAt = DateTime.UtcNow;

            await _coachRepository.UpdateAsync(coach, ct);

            return Result<bool>.Success(true, _operationType);
        }
    }
}

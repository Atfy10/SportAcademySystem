using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
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
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteCoachCommandHandler(
            ICoachRepository coachRepository,
            IUserContextService userContextService,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _coachRepository = coachRepository;
            _userContextService = userContextService;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteCoachCommand request, CancellationToken ct)
        {
            var coach = await _coachRepository.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new IdNotFoundException(nameof(Coach), request.EmployeeId.ToString());

            coach.MarkAsDeleted(_userContextService.UserId.ToString() ?? "System");
            await _coachRepository.UpdateAsync(coach, ct);

            var actorName = _userContextService.UserId is { } actorId
                ? await _userRepository.GetDisplayNameAsync(actorId, ct)
                : "System";
            await _publisher.Publish(
                new EmployeeLifecycleEvent(coach.EmployeeId, $"Employee #{coach.EmployeeId}", "Removed as Coach", actorName),
                ct);

            return Result<bool>.Success(true, _operationType);
        }
    }
}

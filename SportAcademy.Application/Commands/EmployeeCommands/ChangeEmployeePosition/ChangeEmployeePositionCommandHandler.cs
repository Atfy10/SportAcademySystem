using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EmployeeCommands.ChangeEmployeePosition
{
    public class ChangeEmployeePositionCommandHandler : IRequestHandler<ChangeEmployeePositionCommand, Result<EmployeeDto>>
    {
        // How many group names the "still coaches these groups" message lists before it says "...".
        private const int MaxGroupNamesInMessage = 5;

        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public ChangeEmployeePositionCommandHandler(
            IEmployeeRepository employeeRepository,
            ICoachRepository coachRepository,
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _employeeRepository = employeeRepository;
            _coachRepository = coachRepository;
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<EmployeeDto>> Handle(ChangeEmployeePositionCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException($"{request.Id}");

            if (employee.Position == request.Position)
                return Result<EmployeeDto>.Success(EmployeeMapper.ToDto(employee), _operationType);

            // Moving someone out of the Coach position must also remove their Coach record (it is
            // 1:1 with the employee and only meaningful while they hold that position) - but only
            // when that is safe. Checked before anything is touched, so a refusal leaves the
            // employee and the coach record exactly as they were.
            var removeCoachRecord = false;
            if (employee.Position == Position.Coach)
            {
                // Including soft-deleted: an earlier "remove as coach" only flagged the row, and
                // that leftover would otherwise keep occupying the shared Employee/Coach key.
                var coach = await _coachRepository.GetByEmployeeIdIncludingDeletedAsync(employee.Id, cancellationToken);
                if (coach != null)
                {
                    var blockers = await _coachRepository.GetRemovalBlockersAsync(employee.Id, cancellationToken);
                    if (blockers.GroupCount > 0)
                        throw new CoachHasGroupsException(blockers.GroupCount, FormatGroupNames(blockers.GroupNames));
                    if (blockers.TrainingHistoryCount > 0)
                        throw new CoachHasTrainingHistoryException(blockers.TrainingHistoryCount);

                    removeCoachRecord = true;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            employee.Position = request.Position;
            await _employeeRepository.UpdateAsyncWithoutSave(employee, cancellationToken);

            if (removeCoachRecord)
            {
                // One transaction so the position change and the coach-record delete land
                // together - never "position is HR but a coach record still exists" or the reverse.
                await _unitOfWork.InTransactionAsync(async () =>
                {
                    await _coachRepository.HardDeleteAsync(employee.Id, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return true;
                }, cancellationToken);

                var actorName = _userContextService.UserId is { } actorId
                    ? await _userRepository.GetDisplayNameAsync(actorId, cancellationToken)
                    : "System";
                await _publisher.Publish(
                    new EmployeeLifecycleEvent(employee.Id, $"{employee.FirstName} {employee.LastName}", "Removed as Coach", actorName),
                    cancellationToken);
            }
            else
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<EmployeeDto>.Success(EmployeeMapper.ToDto(employee), _operationType);
        }

        private static string FormatGroupNames(IReadOnlyList<string> names)
        {
            var shown = string.Join(", ", names.Take(MaxGroupNamesInMessage));
            return names.Count > MaxGroupNamesInMessage ? $"{shown}, ..." : shown;
        }
    }
}

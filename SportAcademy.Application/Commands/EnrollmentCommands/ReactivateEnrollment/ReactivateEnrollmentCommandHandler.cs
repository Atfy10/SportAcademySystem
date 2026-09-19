using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ReactivateEnrollment;

public class ReactivateEnrollmentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    ITraineeGroupRepository traineeGroupRepository,
    IUserContextService userContext,
    IUserRepository userRepository,
    IPublisher publisher)
    : IRequestHandler<ReactivateEnrollmentCommand, Result<EnrollmentDto>>
{
    public async Task<Result<EnrollmentDto>> Handle(
        ReactivateEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new IdNotFoundException("Enrollment", request.Id.ToString());

        if (enrollment.Status != EnrollmentStatus.Suspended)
            throw new EnrollmentNotSuspendedException(enrollment.Id);

        if (request.SessionRemaining > enrollment.SessionAllowed)
            throw new ArgumentOutOfRangeException(
                nameof(request.SessionRemaining),
                $"Sessions remaining ({request.SessionRemaining}) can't exceed what this enrollment was ever allowed ({enrollment.SessionAllowed}).");

        var group = await traineeGroupRepository.GetByIdWithSchedulesAsync(enrollment.TraineeGroupId, cancellationToken)
            ?? throw new TraineeGroupNotFoundException(enrollment.TraineeGroupId.ToString());

        var trainingDays = group.GroupSchedules.Select(gs => gs.Day).Distinct().ToList();

        // Walked from today, same as CreateEnrollment - the trainee resumes as of whenever
        // staff actually reactivates them, not backdated to when they were suspended.
        enrollment.ExpiryDate = TrainingScheduleService
            .ComputeEndDate(DateOnly.FromDateTime(DateTime.UtcNow), request.SessionRemaining, trainingDays)
            .ToDateTime(TimeOnly.MinValue);
        enrollment.SessionRemaining = request.SessionRemaining;
        enrollment.Status = EnrollmentStatus.Active;

        // No capacity re-check: a suspended enrollment never freed its group slot in the first
        // place (see GetActiveEnrollmentCountForGroupAsync), so there's nothing to re-validate.
        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        var actorName = userContext.UserId is { } userId
            ? await userRepository.GetDisplayNameAsync(userId, cancellationToken)
            : "System";
        await publisher.Publish(new EnrollmentLifecycleEvent(enrollment.Id, "Reactivated", actorName), cancellationToken);

        return Result<EnrollmentDto>.Success(
            EnrollmentMapper.ToDto(enrollment), OperationType.Update.ToString());
    }
}

using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.ExcuseRequestExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.CreateExcuseRequest;

public class CreateExcuseRequestCommandHandler(
    IExcuseRequestRepository excuseRequestRepository,
    ISessionOccurrenceRepository sessionOccurrenceRepository,
    IEnrollmentRepository enrollmentRepository,
    IUserContextService userContext,
    IPublisher publisher)
    : IRequestHandler<CreateExcuseRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateExcuseRequestCommand request, CancellationToken cancellationToken)
    {
        var groupId = await sessionOccurrenceRepository.GetTraineeGroupIdAsync(
            request.SessionOccurrenceId, cancellationToken)
            ?? throw new SessionOccurrenceNotFoundException(request.SessionOccurrenceId.ToString());

        var enrollmentId = await enrollmentRepository.GetEnrollmentIdAsync(
            request.TraineeId, groupId, cancellationToken)
            ?? throw new EnrollmentNotFoundException($"trainee {request.TraineeId} in group {groupId}");

        if (await excuseRequestRepository.ExistsPendingAsync(request.SessionOccurrenceId, request.TraineeId, cancellationToken))
            throw new DuplicateExcuseRequestException(request.SessionOccurrenceId, request.TraineeId);

        var userId = userContext.UserId?.ToString();
        var excuseRequest = new ExcuseRequest
        {
            SessionOccurrenceId = request.SessionOccurrenceId,
            TraineeId = request.TraineeId,
            EnrollmentId = enrollmentId,
            Reason = request.Reason.Trim(),
            Status = ExcuseRequestStatus.Pending,
            RequestedByUserId = userId
        };

        await excuseRequestRepository.AddAsync(excuseRequest, cancellationToken);

        await publisher.Publish(new ExcuseRequestCreatedEvent(excuseRequest.Id), cancellationToken);

        return Result<int>.Success(excuseRequest.Id, OperationType.Add.ToString());
    }
}

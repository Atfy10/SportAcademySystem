using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.ExcuseRequestExceptions;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.RejectExcuseRequest;

public class RejectExcuseRequestCommandHandler(
    IExcuseRequestRepository excuseRequestRepository,
    IUserContextService userContext,
    IPublisher publisher)
    : IRequestHandler<RejectExcuseRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RejectExcuseRequestCommand request, CancellationToken cancellationToken)
    {
        var excuseRequest = await excuseRequestRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ExcuseRequestNotFoundException(request.Id.ToString());

        if (excuseRequest.Status != ExcuseRequestStatus.Pending)
            throw new ExcuseRequestAlreadyReviewedException(excuseRequest.Id, excuseRequest.Status.ToString());

        excuseRequest.Status = ExcuseRequestStatus.Rejected;
        excuseRequest.ReviewedByUserId = userContext.UserId?.ToString();
        excuseRequest.ReviewedAt = DateTime.UtcNow;
        excuseRequest.ReviewNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await excuseRequestRepository.UpdateAsync(excuseRequest, cancellationToken);

        await publisher.Publish(new ExcuseRequestReviewedEvent(excuseRequest.Id, Approved: false), cancellationToken);

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}

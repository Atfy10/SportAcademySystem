using MediatR;

namespace SportAcademy.Application.Events;

public sealed record ExcuseRequestReviewedEvent(int ExcuseRequestId, bool Approved) : INotification;

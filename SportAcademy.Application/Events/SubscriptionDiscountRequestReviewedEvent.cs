using MediatR;

namespace SportAcademy.Application.Events;

public sealed record SubscriptionDiscountRequestReviewedEvent(int RequestId, bool Approved) : INotification;

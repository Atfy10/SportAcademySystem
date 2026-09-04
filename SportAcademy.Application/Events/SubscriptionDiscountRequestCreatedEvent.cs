using MediatR;

namespace SportAcademy.Application.Events;

public sealed record SubscriptionDiscountRequestCreatedEvent(int RequestId) : INotification;

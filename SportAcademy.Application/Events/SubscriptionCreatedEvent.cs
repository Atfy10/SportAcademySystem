using MediatR;

namespace SportAcademy.Application.Events;

public sealed record SubscriptionCreatedEvent(int SubscriptionId, int TraineeId) : INotification;

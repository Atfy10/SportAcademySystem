using MediatR;

namespace SportAcademy.Application.Events;

public sealed record PaymentRefundedEvent(string PaymentNumber, decimal Amount, string ActorName) : INotification;

using MediatR;

namespace SportAcademy.Application.Events;

public sealed record PaymentVoidedEvent(string PaymentNumber, string ActorName) : INotification;

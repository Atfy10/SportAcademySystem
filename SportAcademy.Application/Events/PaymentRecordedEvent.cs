using MediatR;

namespace SportAcademy.Application.Events;

public sealed record PaymentRecordedEvent(string PaymentNumber, decimal Amount, string Currency, string ActorName) : INotification;

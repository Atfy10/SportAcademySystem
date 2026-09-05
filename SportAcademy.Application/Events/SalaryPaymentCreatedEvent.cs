using MediatR;

namespace SportAcademy.Application.Events;

public sealed record SalaryPaymentCreatedEvent(int SalaryPaymentId) : INotification;

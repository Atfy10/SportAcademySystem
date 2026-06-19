using MediatR;

namespace SportAcademy.Application.Events;

public sealed record EnrollmentCreatedEvent(int EnrollmentId) : INotification;

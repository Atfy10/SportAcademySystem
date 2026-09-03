using MediatR;

namespace SportAcademy.Application.Events;

public sealed record EmployeeStatusChangedEvent(int EmployeeId, bool IsWork, string ActorName) : INotification;

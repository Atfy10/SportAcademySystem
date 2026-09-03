using MediatR;

namespace SportAcademy.Application.Events;

/// Covers Create/Delete on an Employee (and, via the same shape, a Coach - a Coach is an
/// Employee sub-record) - one shared event, differing only by which action happened.
public sealed record EmployeeLifecycleEvent(int EmployeeId, string EmployeeName, string Action, string ActorName) : INotification;

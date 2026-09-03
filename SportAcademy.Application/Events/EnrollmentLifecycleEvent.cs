using MediatR;

namespace SportAcademy.Application.Events;

/// Covers Suspend/Activate/Delete on an Enrollment - one shared event, differing only by which
/// action happened.
public sealed record EnrollmentLifecycleEvent(int EnrollmentId, string Action, string ActorName) : INotification;
